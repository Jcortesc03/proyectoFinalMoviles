using System.Reflection;

namespace MauiApp1.Configuration
{
    /// <summary>
    /// Lee las variables de entorno de la aplicacion y las traduce a claves de
    /// configuracion (<c>Seccion:Propiedad</c>) para sobreescribir <c>appsettings.json</c>.
    /// </summary>
    /// <remarks>
    /// Fuentes, de menor a mayor prioridad:
    /// <list type="number">
    /// <item><c>Configuration/.env</c> embebido en el ensamblado (si existe al compilar).</item>
    /// <item>Variables de entorno del proceso (<see cref="Environment.GetEnvironmentVariable(string)"/>).</item>
    /// </list>
    /// En Android/iOS no hay variables de entorno de usuario, por eso se usa el archivo <c>.env</c>.
    /// </remarks>
    public static class EnvironmentVariables
    {
        public const string AnthropicApiKey = "ANTHROPIC_API_KEY";
        public const string ClaudeModel = "CLAUDE_MODEL";
        public const string ClaudeFallbackModel = "CLAUDE_FALLBACK_MODEL";
        public const string ClaudeMaxTokens = "CLAUDE_MAX_TOKENS";
        public const string ClaudeTimeoutSeconds = "CLAUDE_TIMEOUT_SECONDS";

        private const string EnvResourceName = "MauiApp1.Configuration.env";

        /// <summary>Relacion variable de entorno -> clave de configuracion.</summary>
        private static readonly Dictionary<string, string> Mapeo = new()
        {
            [AnthropicApiKey] = "Claude:ApiKey",
            [ClaudeModel] = "Claude:Model",
            [ClaudeFallbackModel] = "Claude:FallbackModel",
            [ClaudeMaxTokens] = "Claude:MaxTokens",
            [ClaudeTimeoutSeconds] = "Claude:TimeoutSeconds",
        };

        /// <summary>
        /// Devuelve las claves de configuracion definidas por variables de entorno
        /// (solo las que tienen valor), listas para <c>AddInMemoryCollection</c>.
        /// </summary>
        public static Dictionary<string, string?> Load()
        {
            var archivoEnv = ReadEmbeddedEnvFile();
            var resultado = new Dictionary<string, string?>();

            foreach (var (variable, clave) in Mapeo)
            {
                var valor = Environment.GetEnvironmentVariable(variable);
                if (string.IsNullOrWhiteSpace(valor))
                    archivoEnv.TryGetValue(variable, out valor);

                if (!string.IsNullOrWhiteSpace(valor))
                    resultado[clave] = valor;
            }

            return resultado;
        }

        /// <summary>Lee el recurso embebido <c>.env</c> (formato <c>CLAVE=valor</c>, <c>#</c> para comentarios).</summary>
        private static Dictionary<string, string> ReadEmbeddedEnvFile()
        {
            var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EnvResourceName);
            if (stream is null)
                return variables;

            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } linea)
            {
                linea = linea.Trim();
                if (linea.Length == 0 || linea.StartsWith('#'))
                    continue;

                var separador = linea.IndexOf('=');
                if (separador <= 0)
                    continue;

                var clave = linea[..separador].Trim();
                var valor = linea[(separador + 1)..].Trim().Trim('"', '\'');
                variables[clave] = valor;
            }

            return variables;
        }
    }
}
