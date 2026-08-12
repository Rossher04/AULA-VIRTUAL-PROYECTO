using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class RoleHomePage : ContentPage
{
    public RoleHomePage(string title, string message = "")
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        // Sin mensaje, se oculta la etiqueta para no dejar un hueco en la tarjeta.
        MessageLabel.IsVisible = !string.IsNullOrWhiteSpace(message);
        UserLabel.Text = $"Sesión: {SessionService.Usuario} ({SessionService.Rol})";
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        SessionService.Clear();
        await Navigation.PopToRootAsync();
    }
}
