using System.Text.Json.Nodes;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class CrearDocentePage : ContentPage
{
    private readonly List<int> _institucionIds = new();

    public CrearDocentePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarInstitucionesAsync();
    }

    private async Task CargarInstitucionesAsync()
    {
        SetLoading(true);
        var result = await ApiClient.GetAsync(ApiTarget.Aula, "instituciones/");
        SetLoading(false);

        if (!result.Ok)
        {
            ErrorLabel.ShowError(result.ErrorMessage ?? "No se pudieron cargar las instituciones.");
            return;
        }

        InstitucionPicker.Items.Clear();
        _institucionIds.Clear();

        foreach (var node in result.Data.AsJsonArray())
        {
            if (node is not JsonObject obj) continue;
            InstitucionPicker.Items.Add(obj.GetStr("nombre"));
            _institucionIds.Add(obj.GetIntOrZero("id"));
        }

        if (InstitucionPicker.Items.Count > 0)
            InstitucionPicker.SelectedIndex = 0;
    }

    private async void OnCrearClicked(object sender, EventArgs e)
    {
        ErrorLabel.HideError();

        var nombre = NombreEntry.Text?.Trim();
        var apellido = ApellidoEntry.Text?.Trim();
        var usuario = UsuarioEntry.Text?.Trim();
        var contrasena = ContrasenaEntry.Text;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) ||
            string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            ErrorLabel.ShowError("Completa nombre, apellido, usuario y contrasena.");
            return;
        }

        if (InstitucionPicker.SelectedIndex < 0)
        {
            ErrorLabel.ShowError("Selecciona una institucion.");
            return;
        }

        var institucionAulaId = _institucionIds[InstitucionPicker.SelectedIndex];

        SetLoading(true);

        // Paso 1: crear el usuario en la API de Login, con rol DOCENTE
        var usuarioBody = new JsonObject
        {
            ["institucion"] = SessionService.IdInstitucion,
            ["usuario"] = usuario,
            ["contrasena"] = contrasena,
            ["rol_tipo"] = "DOCENTE",
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

        // Paso 2: crear el registro de docente en la API de Aula, referenciando ese id_usuario
        var docenteBody = new JsonObject
        {
            ["institucion"] = institucionAulaId,
            ["nombre"] = nombre,
            ["apellido"] = apellido,
            ["id_usuario"] = idUsuarioCreado
        };

        var docenteResult = await ApiClient.PostAsync(ApiTarget.Aula, "docentes/", docenteBody);

        SetLoading(false);

        if (!docenteResult.Ok)
        {
            // El usuario de login ya se creo pero el docente no se pudo registrar en Aula.
            // Intentamos revertir el usuario de login para no dejar cuentas huerfanas.
            await ApiClient.DeleteAsync(ApiTarget.Login, $"usuarios/{idUsuarioCreado}/");

            ErrorLabel.ShowError("El usuario se creo pero no se pudo registrar el catedratico en Aula.\n" +
                         (docenteResult.ErrorMessage ?? "") +
                         "\nSe revirtio la creacion del usuario, intenta de nuevo.");
            return;
        }

        await DisplayAlert("Listo", $"Catedratico '{nombre} {apellido}' creado con usuario '{usuario}'.", "OK");
        await Navigation.PopAsync();
    }

    private void SetLoading(bool loading) => LoadingIndicator.SetLoading(loading);
}
