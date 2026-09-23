namespace MauiApp1.Configuration
{
    /// <summary>
    /// Configuracion raiz de la aplicacion, obtenida desde <c>appsettings.json</c>.
    /// </summary>
    public class AppSettings
    {
        public DatabaseSettings Database { get; set; } = new();
        public AiApiSettings AiApi { get; set; } = new();
    }

    /// <summary>
    /// Configuracion de la base de datos SQLite.
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>Nombre del archivo de la base de datos dentro de AppDataDirectory.</summary>
        public string DatabaseFileName { get; set; } = "mauiapp1.db3";

        /// <summary>Si es true, elimina la base de datos al iniciar la aplicacion (uso en desarrollo).</summary>
        public bool DeleteOnStartup { get; set; } = false;

        /// <summary>Habilita las claves foraneas de SQLite para respetar las relaciones entre tablas.</summary>
        public bool EnforceForeignKeys { get; set; } = true;
    }

    /// <summary>
    /// Configuracion del cliente HTTP hacia la API de IA externa.
    /// </summary>
    public class AiApiSettings
    {
        /// <summary>URL base de la API.</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Clave de autenticacion (Bearer). No compromenter ni versionar.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Modelo por defecto para las consultas.</summary>
        public string DefaultModel { get; set; } = string.Empty;

        /// <summary>Timeout de las llamadas HTTP en segundos.</summary>
        public int TimeoutSeconds { get; set; } = 60;
    }
}