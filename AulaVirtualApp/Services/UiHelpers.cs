namespace AulaVirtualApp.Services;

/// <summary>
/// Pequenas extensiones para los tres gestos repetidos en (casi) todas las paginas:
/// mostrar/ocultar el indicador de carga, mostrar un mensaje de error, y leer texto
/// de un selector que puede fallar (datos con forma inesperada del backend).
/// </summary>
public static class UiHelpers
{
    public static void SetLoading(this ActivityIndicator indicator, bool loading)
    {
        indicator.IsVisible = loading;
        indicator.IsRunning = loading;
    }

    public static void ShowError(this Label label, string mensaje)
    {
        label.Text = mensaje;
        label.IsVisible = true;
    }

    public static void HideError(this Label label)
    {
        label.IsVisible = false;
    }

    public static string SafeText(Func<string> selector)
    {
        try { return selector() ?? string.Empty; }
        catch { return string.Empty; }
    }
}
