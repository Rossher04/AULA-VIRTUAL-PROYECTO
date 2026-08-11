using System.Text.Json.Nodes;
using AulaVirtualApp.Models;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class EntityFormPage : ContentPage
{
    private class RuntimeField
    {
        public FieldConfig Config { get; set; } = null!;
        public View Control { get; set; } = null!;
        public List<object> PickerValues { get; set; } = new();
    }

    private readonly EntityConfig _config;
    private readonly JsonObject? _existing;
    private readonly bool _isEdit;
    private readonly List<RuntimeField> _runtimeFields = new();
    private bool _initialized;
    private Label _errorLabel = null!;

    public EntityFormPage(EntityConfig config, JsonObject? existing)
    {
        InitializeComponent();
        _config = config;
        _existing = existing;
        _isEdit = existing != null;
        Title = (_isEdit ? "Editar: " : "Nuevo: ") + _config.Title;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_initialized)
            return;
        _initialized = true;
        await BuildFormAsync();
    }

    private async Task BuildFormAsync()
    {
        SetLoading(true);

        foreach (var field in _config.Fields)
        {
            // Campos "solo al crear" (ej. id_usuario) no se muestran al editar,
            // para no permitir romper el vinculo con el usuario de login desde aqui.
            if (field.OnlyOnCreate && _isEdit)
                continue;

            View control;

            switch (field.Type)
            {
                case FieldType.Password:
                    control = new Entry { Placeholder = field.Label, IsPassword = true };
                    break;

                case FieldType.Number:
                    control = new Entry { Placeholder = field.Label, Keyboard = Keyboard.Numeric };
                    break;

                case FieldType.Decimal:
                    control = new Entry { Placeholder = field.Label, Keyboard = Keyboard.Numeric };
                    break;

                case FieldType.Bool:
                    control = new Switch();
                    break;

                case FieldType.Date:
                    control = new DatePicker();
                    break;

                case FieldType.Time:
                    control = new TimePicker();
                    break;

                case FieldType.Picker:
                case FieldType.StaticChoice:
                    control = new Picker { Title = $"Selecciona {field.Label}" };
                    break;

                default:
                    control = new Entry { Placeholder = field.Label };
                    break;
            }

            var runtime = new RuntimeField { Config = field, Control = control };
            _runtimeFields.Add(runtime);

            FieldsContainer.Children.Add(new Label { Text = field.Label, Style = (Style)Application.Current!.Resources["FieldLabel"] });
            FieldsContainer.Children.Add(control);
        }

        // Cargar opciones de los pickers (algunos dependen de otro endpoint remoto)
        foreach (var runtime in _runtimeFields)
        {
            if (runtime.Config.Type == FieldType.StaticChoice)
            {
                var picker = (Picker)runtime.Control;
                foreach (var (value, optionLabel) in runtime.Config.StaticOptions ?? new())
                {
                    picker.Items.Add(optionLabel);
                    runtime.PickerValues.Add(value);
                }
            }
            else if (runtime.Config.Type == FieldType.Picker)
            {
                await LoadPickerOptionsAsync(runtime);
            }
        }

        // Si estamos editando, precargar los valores actuales en cada control
        if (_isEdit && _existing != null)
        {
            FillExistingValues();
        }

        // Boton Guardar
        var guardarBtn = new Button { Text = "Guardar", Margin = new Thickness(0, 10, 0, 0) };
        guardarBtn.Clicked += OnGuardarClicked;
        FieldsContainer.Children.Add(guardarBtn);

        _errorLabel = new Label { Style = (Style)Application.Current!.Resources["ErrorLabel"], IsVisible = false };
        FieldsContainer.Children.Add(_errorLabel);

        SetLoading(false);
    }

    private async Task LoadPickerOptionsAsync(RuntimeField runtime)
    {
        var picker = (Picker)runtime.Control;
        var result = await ApiClient.GetAsync(runtime.Config.SourceApiTarget, runtime.Config.SourceEndpoint);

        if (!result.Ok)
        {
            picker.Items.Add("(no se pudo cargar)");
            return;
        }

        foreach (var node in result.Data.AsJsonArray())
        {
            if (node is not JsonObject obj)
                continue;

            var optionText = runtime.Config.OptionLabel != null
                ? UiHelpers.SafeText(() => runtime.Config.OptionLabel!(obj))
                : obj.GetStr("nombre");

            picker.Items.Add(string.IsNullOrWhiteSpace(optionText) ? "(sin nombre)" : optionText);

            object value;
            if (obj.TryGetPropertyValue(runtime.Config.ValueField, out var valNode) && valNode != null)
            {
                if (int.TryParse(valNode.ToString(), out var intVal))
                    value = intVal;
                else
                    value = valNode.ToString();
            }
            else
            {
                value = string.Empty;
            }
            runtime.PickerValues.Add(value);
        }
    }

    private void FillExistingValues()
    {
        foreach (var runtime in _runtimeFields)
        {
            if (!_existing!.TryGetPropertyValue(runtime.Config.Key, out var existingVal) || existingVal == null)
                continue;

            var existingText = existingVal.ToString();

            switch (runtime.Config.Type)
            {
                case FieldType.Text:
                case FieldType.Password:
                    ((Entry)runtime.Control).Text = existingText;
                    break;
                case FieldType.Number:
                case FieldType.Decimal:
                    ((Entry)runtime.Control).Text = existingText;
                    break;
                case FieldType.Bool:
                    ((Switch)runtime.Control).IsToggled = existingText == "true" || existingText == "True";
                    break;
                case FieldType.Date:
                    if (DateTime.TryParse(existingText, out var dateVal))
                        ((DatePicker)runtime.Control).Date = dateVal;
                    break;
                case FieldType.Time:
                    if (TimeSpan.TryParse(existingText, out var timeVal))
                        ((TimePicker)runtime.Control).Time = timeVal;
                    break;
                case FieldType.Picker:
                case FieldType.StaticChoice:
                    var idx = runtime.PickerValues.FindIndex(v => v.ToString() == existingText);
                    if (idx >= 0)
                        ((Picker)runtime.Control).SelectedIndex = idx;
                    break;
            }
        }
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        _errorLabel.HideError();

        var body = new JsonObject();
        var faltantes = new List<string>();

        // Los campos "solo al crear" no tienen control visible al editar, pero el
        // backend igual los exige en el PUT (son NOT NULL): se reenvia su valor
        // original sin permitir que el usuario lo toque desde este formulario.
        if (_isEdit && _existing != null)
        {
            foreach (var field in _config.Fields)
            {
                if (field.OnlyOnCreate && _existing.TryGetPropertyValue(field.Key, out var existingVal) && existingVal != null)
                {
                    body[field.Key] = existingVal.DeepClone();
                }
            }
        }

        foreach (var runtime in _runtimeFields)
        {
            switch (runtime.Config.Type)
            {
                case FieldType.Text:
                case FieldType.Password:
                {
                    var text = ((Entry)runtime.Control).Text;
                    if (runtime.Config.Required && string.IsNullOrWhiteSpace(text))
                    {
                        faltantes.Add(runtime.Config.Label);
                        break;
                    }
                    if (!string.IsNullOrEmpty(text))
                        body[runtime.Config.Key] = text;
                    break;
                }
                case FieldType.Number:
                {
                    var text = ((Entry)runtime.Control).Text;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        if (runtime.Config.Required) faltantes.Add(runtime.Config.Label);
                        break;
                    }
                    if (int.TryParse(text, out var intVal))
                        body[runtime.Config.Key] = intVal;
                    else
                        faltantes.Add($"{runtime.Config.Label} (debe ser numero entero)");
                    break;
                }
                case FieldType.Decimal:
                {
                    var text = ((Entry)runtime.Control).Text;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        if (runtime.Config.Required) faltantes.Add(runtime.Config.Label);
                        break;
                    }
                    if (decimal.TryParse(text, out var decVal))
                        body[runtime.Config.Key] = decVal;
                    else
                        faltantes.Add($"{runtime.Config.Label} (debe ser numero)");
                    break;
                }
                case FieldType.Bool:
                    body[runtime.Config.Key] = ((Switch)runtime.Control).IsToggled;
                    break;
                case FieldType.Date:
                    body[runtime.Config.Key] = $"{((DatePicker)runtime.Control).Date:yyyy-MM-dd}";
                    break;
                case FieldType.Time:
                {
                    var time = ((TimePicker)runtime.Control).Time;
                    body[runtime.Config.Key] = $"{time:hh\\:mm}";
                    break;
                }
                case FieldType.Picker:
                case FieldType.StaticChoice:
                {
                    var picker = (Picker)runtime.Control;
                    if (picker.SelectedIndex < 0 || picker.SelectedIndex >= runtime.PickerValues.Count)
                    {
                        if (runtime.Config.Required) faltantes.Add(runtime.Config.Label);
                        break;
                    }
                    var value = runtime.PickerValues[picker.SelectedIndex];
                    if (value is int intVal)
                        body[runtime.Config.Key] = intVal;
                    else
                        body[runtime.Config.Key] = value.ToString();
                    break;
                }
            }
        }

        if (faltantes.Count > 0)
        {
            _errorLabel.ShowError("Falta completar: " + string.Join(", ", faltantes));
            return;
        }

        SetLoading(true);

        var result = _isEdit
            ? await ApiClient.PutAsync(_config.ApiTarget, $"{_config.Endpoint}{_existing!.GetStr("id")}/", body)
            : await ApiClient.PostAsync(_config.ApiTarget, _config.Endpoint, body);

        SetLoading(false);

        if (!result.Ok)
        {
            _errorLabel.ShowError(result.ErrorMessage ?? "No se pudo guardar el registro.");
            return;
        }

        await Navigation.PopAsync();
    }

    private void SetLoading(bool loading) => LoadingIndicator.SetLoading(loading);
}
