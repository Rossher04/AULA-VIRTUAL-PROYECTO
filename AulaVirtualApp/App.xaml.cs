using AulaVirtualApp.Pages;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace AulaVirtualApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // La app tiene una sola paleta (clara). Sin esto, MAUI sigue el tema del
        // sistema y, en un telefono con modo oscuro activado, Android pinta de
        // oscuro los controles nativos (Entry, Picker, DatePicker) y el fondo de
        // la ventana, ignorando los estilos definidos en Styles.xaml.
        UserAppTheme = AppTheme.Light;

        MainPage = new NavigationPage(new LoginPage())
        {
            BarBackgroundColor = Color.FromArgb("#0A402C"),
            BarTextColor = Colors.White
        };
    }
}
