using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class RoleHomePage : ContentPage
{
    public RoleHomePage(string title, string message)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        UserLabel.Text = $"Sesion: {SessionService.Usuario} ({SessionService.Rol})";
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        SessionService.Clear();
        await Navigation.PopToRootAsync();
    }
}
