using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LoginUrlEntry.Text = ApiConfig.LoginBaseUrl;
        AulaUrlEntry.Text = ApiConfig.AulaBaseUrl;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(LoginUrlEntry.Text))
            ApiConfig.LoginBaseUrl = LoginUrlEntry.Text;

        if (!string.IsNullOrWhiteSpace(AulaUrlEntry.Text))
            ApiConfig.AulaBaseUrl = AulaUrlEntry.Text;

        await DisplayAlert("Guardado", "La configuración se guardó correctamente.", "OK");
        await Navigation.PopAsync();
    }
}
