namespace MauiApp1.Configuration
{
    /// <summary>
    /// Configuracion raiz de la aplicacion, obtenida desde <c>appsettings.json</c>
    /// y sobreescrita por variables de entorno (ver <see cref="EnvironmentVariables"/>).
    /// </summary>
    public class AppSettings
    {
        public DatabaseSettings Database { get; set; } = new();
        public ClaudeSettings Claude { get; set; } = new();
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
    /// Configuracion del cliente de la API de Claude (Anthropic).
    /// Los valores se pueden sobreescribir con las variables de entorno <c>ANTHROPIC_API_KEY</c> y <c>CLAUDE_*</c>.
    /// </summary>
    public class ClaudeSettings
    {
        /// <summary>Clave de la API de Anthropic. Variable: <c>ANTHROPIC_API_KEY</c>. No versionar.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Modelo usado en las consultas. Variable: <c>CLAUDE_MODEL</c>.</summary>
        public string Model { get; set; } = "claude-opus-5";

        /// <summary>Modelo que responde si el principal rechaza la consulta. Variable: <c>CLAUDE_FALLBACK_MODEL</c>.</summary>
        public string FallbackModel { get; set; } = "claude-opus-4-8";

        /// <summary>Maximo de tokens de la respuesta. Variable: <c>CLAUDE_MAX_TOKENS</c>.</summary>
        public int MaxTokens { get; set; } = 16000;

        /// <summary>Timeout de las llamadas HTTP en segundos. Variable: <c>CLAUDE_TIMEOUT_SECONDS</c>.</summary>
        public int TimeoutSeconds { get; set; } = 120;

        /// <summary>Instruccion de sistema enviada en cada consulta (fuerza respuestas en texto plano).</summary>
        public string SystemInstruction { get; set; } =
            "Eres un buscador. Responde en espanol, en texto plano, sin formato Markdown.";
    }
}
