using System.Text.Json.Nodes;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class CrearEstudiantePage : ContentPage
{
    private readonly List<int> _institucionIds = new();
    private readonly List<int> _carreraIds = new();

    public CrearEstudiantePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarCombosAsync();
    }

    private async Task CargarCombosAsync()
    {
        SetLoading(true);

        var institucionesResult = await ApiClient.GetAsync(ApiTarget.Aula, "instituciones/");
        InstitucionPicker.Items.Clear();
        _institucionIds.Clear();
        if (institucionesResult.Ok)
        {
            foreach (var node in institucionesResult.Data.AsJsonArray())
            {
                if (node is not JsonObject obj) continue;
                InstitucionPicker.Items.Add(obj.GetStr("nombre"));
                _institucionIds.Add(obj.GetIntOrZero("id"));
            }
            if (InstitucionPicker.Items.Count > 0)
                InstitucionPicker.SelectedIndex = 0;
        }

        var carrerasResult = await ApiClient.GetAsync(ApiTarget.Aula, "carreras/");
        CarreraPicker.Items.Clear();
        _carreraIds.Clear();
        if (carrerasResult.Ok)
        {
            foreach (var node in carrerasResult.Data.AsJsonArray())
            {
                if (node is not JsonObject obj) continue;
                CarreraPicker.Items.Add(obj.GetStr("nombre"));
                _carreraIds.Add(obj.GetIntOrZero("id"));
            }
        }

        SetLoading(false);

        if (!institucionesResult.Ok)
            ErrorLabel.ShowError(institucionesResult.ErrorMessage ?? "No se pudieron cargar las instituciones.");
        else if (!carrerasResult.Ok)
            ErrorLabel.ShowError(carrerasResult.ErrorMessage ?? "No se pudieron cargar las carreras.");
        else if (CarreraPicker.Items.Count == 0)
            ErrorLabel.ShowError("Todavia no hay carreras creadas. Crea al menos una carrera antes de registrar alumnos.");
    }

    private async void OnCrearClicked(object sender, EventArgs e)
    {
        ErrorLabel.HideError();

        var nombre = NombreEntry.Text?.Trim();
        var apellido = ApellidoEntry.Text?.Trim();
        var carne = CarneEntry.Text?.Trim();
        var usuario = UsuarioEntry.Text?.Trim();
        var contrasena = ContrasenaEntry.Text;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) ||
            string.IsNullOrWhiteSpace(carne) || string.IsNullOrWhiteSpace(usuario) ||
            string.IsNullOrWhiteSpace(contrasena))
        {
            ErrorLabel.ShowError("Completa nombre, apellido, carne, usuario y contrasena.");
            return;
        }

        if (InstitucionPicker.SelectedIndex < 0)
        {
            ErrorLabel.ShowError("Selecciona una institucion.");
            return;
        }

        if (CarreraPicker.SelectedIndex < 0)
        {
            ErrorLabel.ShowError("Selecciona una carrera.");
            return;
        }

        var institucionAulaId = _institucionIds[InstitucionPicker.SelectedIndex];
        var carreraId = _carreraIds[CarreraPicker.SelectedIndex];

        SetLoading(true);

        // Paso 1: crear el usuario en la API de Login, con rol ESTUDIANTE
        var usuarioBody = new JsonObject
        {
            ["institucion"] = SessionService.IdInstitucion,
            ["usuario"] = usuario,
            ["contrasena"] = contrasena,
            ["rol_tipo"] = "ESTUDIANTE",
            ["activo"] = true
        };

        var usuarioResult = await ApiClient.PostAsync(ApiTarget.Login, "usuarios/", usuarioBody);

        if (!usuarioResult.Ok || usuarioResult.Data is not JsonObject usuarioData)
        {
            SetLoading(false);
            ErrorLabel.ShowError("No se pudo crear el usuario de acceso.\n" + (usuarioResult.ErrorMessage ?? ""));
            return;
        }

        var idUsuarioCreado = usuarioData.GetIntOrZero("id");

        // Paso 2: crear el registro de estudiante en la API de Aula
        var estudianteBody = new JsonObject
        {
            ["institucion"] = institucionAulaId,
            ["nombre"] = nombre,
            ["apellido"] = apellido,
            ["carne"] = carne,
            ["carrera"] = carreraId,
            ["id_usuario"] = idUsuarioCreado
        };

        var estudianteResult = await ApiClient.PostAsync(ApiTarget.Aula, "estudiantes/", estudianteBody);

        SetLoading(false);

        if (!estudianteResult.Ok)
        {
            await ApiClient.DeleteAsync(ApiTarget.Login, $"usuarios/{idUsuarioCreado}/");

            ErrorLabel.ShowError("El usuario se creo pero no se pudo registrar el alumno en Aula.\n" +
                         (estudianteResult.ErrorMessage ?? "") +
                         "\nSe revirtio la creacion del usuario, intenta de nuevo.");
            return;
        }

        await DisplayAlert("Listo", $"Alumno '{nombre} {apellido}' creado con usuario '{usuario}'.", "OK");
        await Navigation.PopAsync();
    }

    private void SetLoading(bool loading) => LoadingIndicator.SetLoading(loading);
}
