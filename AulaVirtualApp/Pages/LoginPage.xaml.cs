using System.Text.Json.Nodes;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SessionService.Clear();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.HideError();

        var usuario = UsuarioEntry.Text?.Trim();
        var contrasena = ContrasenaEntry.Text;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            ErrorLabel.ShowError("Debes ingresar usuario y contraseña.");
            return;
        }

        SetLoading(true);

        var body = new JsonObject
        {
            ["usuario"] = usuario,
            ["contrasena"] = contrasena,
            ["dominio"] = "umes"
        };

        var result = await ApiClient.PostAsync(ApiTarget.Login, "login/", body, authorize: false);

        SetLoading(false);

        if (!result.Ok || result.Data is not JsonObject data)
        {
            ErrorLabel.ShowError(result.ErrorMessage ?? "No se pudo iniciar sesión.");
            return;
        }

        var token = data["token"]?.ToString();
        var contexto = data["contexto"] as JsonObject;

        if (string.IsNullOrEmpty(token) || contexto == null)
        {
            ErrorLabel.ShowError("Respuesta de login incompleta.");
            return;
        }

        SessionService.Token = token;
        SessionService.Usuario = contexto.GetStr("usuario");
        SessionService.Rol = contexto.GetStr("rol");
        SessionService.IdUsuario = contexto.GetIntOrZero("id_usuario");
        SessionService.IdInstitucion = contexto.GetIntOrZero("id_institucion");
        SessionService.InstitucionNombre = contexto.GetStr("institucion");

        switch (SessionService.Rol)
        {
            case "ADMINISTRADOR":
                await Navigation.PushAsync(new AdminDashboardPage());
                break;
            case "DOCENTE":
                await Navigation.PushAsync(new RoleHomePage("Panel del Docente"));
                break;
            case "ESTUDIANTE":
                await Navigation.PushAsync(new RoleHomePage("Panel del Estudiante"));
                break;
            default:
                ErrorLabel.ShowError($"Rol '{SessionService.Rol}' no reconocido.");
                break;
        }
    }

    private void SetLoading(bool loading)
    {
        LoadingIndicator.SetLoading(loading);
        LoginButton.IsEnabled = !loading;
    }
}
