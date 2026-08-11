namespace AulaVirtualApp.Services;

/// <summary>
/// Guarda el contexto de la sesion actual: el token firmado que entrega la API de Login
/// y los datos del "contexto" (id_usuario, id_institucion, usuario, rol) que la API de Aula
/// tambien acepta para validar cada request (Authorization: Bearer TOKEN).
/// </summary>
public static class SessionService
{
    private const string TokenKey = "session_token";
    private const string UsuarioKey = "session_usuario";
    private const string RolKey = "session_rol";
    private const string IdUsuarioKey = "session_id_usuario";
    private const string IdInstitucionKey = "session_id_institucion";
    private const string InstitucionKey = "session_institucion_nombre";

    public static string Token
    {
        get => Preferences.Default.Get(TokenKey, string.Empty);
        set => Preferences.Default.Set(TokenKey, value);
    }

    public static string Usuario
    {
        get => Preferences.Default.Get(UsuarioKey, string.Empty);
        set => Preferences.Default.Set(UsuarioKey, value);
    }

    public static string Rol
    {
        get => Preferences.Default.Get(RolKey, string.Empty);
        set => Preferences.Default.Set(RolKey, value);
    }

    public static int IdUsuario
    {
        get => Preferences.Default.Get(IdUsuarioKey, 0);
        set => Preferences.Default.Set(IdUsuarioKey, value);
    }

    public static int IdInstitucion
    {
        get => Preferences.Default.Get(IdInstitucionKey, 0);
        set => Preferences.Default.Set(IdInstitucionKey, value);
    }

    public static string InstitucionNombre
    {
        get => Preferences.Default.Get(InstitucionKey, string.Empty);
        set => Preferences.Default.Set(InstitucionKey, value);
    }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public static bool IsAdministrador => Rol == "ADMINISTRADOR";
    public static bool IsDocente => Rol == "DOCENTE";
    public static bool IsEstudiante => Rol == "ESTUDIANTE";

    public static void Clear()
    {
        Token = string.Empty;
        Usuario = string.Empty;
        Rol = string.Empty;
        IdUsuario = 0;
        IdInstitucion = 0;
        InstitucionNombre = string.Empty;
    }
}
