namespace AulaVirtualApp.Services;

/// <summary>
/// Which of the two independent Django APIs a request targets.
/// </summary>
public enum ApiTarget
{
    Login,
    Aula
}

/// <summary>
/// Holds and persists the base URLs of the two backend APIs (login y aula).
/// Cambia estos valores desde la pantalla de Configuracion dentro de la app,
/// segun donde este corriendo cada API (PC, emulador Android, dispositivo fisico,
/// o el API publicada en la nube). Los valores se guardan con Preferences, asi que
/// sobreviven a reinicios de la app.
/// </summary>
public static class ApiConfig
{
    private const string LoginUrlKey = "api_login_base_url";
    private const string AulaUrlKey = "api_aula_base_url";

    // APIs publicadas en Render (ver /render.yaml y /DEPLOY.md en la raiz del
    // proyecto). Si el nombre de servicio cambio al desplegar, actualiza estas
    // URLs; siguen sirviendo como respaldo si el usuario nunca configuro nada
    // desde SettingsPage. Para correr contra el backend local en desarrollo,
    // usa SettingsPage y apunta a http://127.0.0.1:8001/api/ y :8002/api/.
    private const string DefaultLoginUrl = "https://aulavirtual-umes-login.onrender.com/api/";
    private const string DefaultAulaUrl = "https://aulavirtual-umes-academico.onrender.com/api/";

    public static string LoginBaseUrl
    {
        get => Preferences.Default.Get(LoginUrlKey, DefaultLoginUrl);
        set => Preferences.Default.Set(LoginUrlKey, NormalizeUrl(value));
    }

    public static string AulaBaseUrl
    {
        get => Preferences.Default.Get(AulaUrlKey, DefaultAulaUrl);
        set => Preferences.Default.Set(AulaUrlKey, NormalizeUrl(value));
    }

    public static string BaseUrlFor(ApiTarget target) =>
        target == ApiTarget.Login ? LoginBaseUrl : AulaBaseUrl;

    private static string NormalizeUrl(string url)
    {
        url = url.Trim();
        return url.EndsWith("/") ? url : url + "/";
    }
}

