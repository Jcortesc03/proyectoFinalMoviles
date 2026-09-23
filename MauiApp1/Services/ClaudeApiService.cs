using System.Text.Json;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Messages;
using MauiApp1.Configuration;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    /// <summary>
    /// Cliente de la API de Claude (Anthropic) basado en el SDK oficial <c>Anthropic</c>.
    /// Envia la consulta del usuario y devuelve la respuesta en texto plano
    /// (el buscador se comporta como un navegador que solo devuelve texto).
    /// </summary>
    /// <remarks>
    /// La clave se toma de la variable de entorno <c>ANTHROPIC_API_KEY</c>
    /// (ver <see cref="EnvironmentVariables"/>). Se usa el endpoint beta de mensajes para
    /// activar el respaldo del lado del servidor: si el modelo principal rechaza la consulta,
    /// responde <see cref="ClaudeSettings.FallbackModel"/> en la misma llamada.
    /// </remarks>
    public sealed class ClaudeApiService
    {
        private readonly ClaudeSettings _settings;
        private readonly AnthropicClient? _client;

        /// <summary>Modelo configurado para las consultas.</summary>
        public string Modelo => _settings.Model;

        /// <summary>Indica si hay una clave de API configurada.</summary>
        public bool EstaConfigurado => _client is not null;

        public ClaudeApiService(AppConfigurationService configuration)
        {
            _settings = configuration.Settings.Claude;

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _client = new AnthropicClient
                {
                    ApiKey = _settings.ApiKey,
                    Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds),
                };
            }
        }

        /// <summary>
        /// Envia <paramref name="prompt"/> a Claude y devuelve la respuesta en texto plano.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si no se definio <c>ANTHROPIC_API_KEY</c>, la clave es invalida, se supero el limite
        /// de uso, Claude rechazo la consulta o no devolvio texto.
        /// </exception>
        /// <exception cref="AnthropicApiException">Otros errores devueltos por la API.</exception>
        public async Task<ClaudeResultado> AskAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (_client is null)
                throw new InvalidOperationException(
                    $"No se encontro la clave de Claude. Define la variable de entorno {EnvironmentVariables.AnthropicApiKey} " +
                    "o agregala en Configuration/.env.");

            var parametros = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = _settings.MaxTokens,
                System = _settings.SystemInstruction,
                Messages = [new() { Role = Role.User, Content = prompt }],
                Betas = [AnthropicBeta.ServerSideFallback2026_06_01],
                Fallbacks = new List<BetaFallbackParam> { new() { Model = _settings.FallbackModel } },
            };

            BetaMessage respuesta;
            try
            {
                respuesta = await _client.Beta.Messages.Create(parametros, cancellationToken);
            }
            catch (AnthropicUnauthorizedException)
            {
                throw new InvalidOperationException(
                    $"La clave de Claude no es valida. Revisa {EnvironmentVariables.AnthropicApiKey}.");
            }
            catch (AnthropicRateLimitException)
            {
                throw new InvalidOperationException(
                    "Se supero el limite de consultas a Claude. Espera un momento e intenta de nuevo.");
            }

            if (respuesta.StopReason == "refusal")
                throw new InvalidOperationException(
                    $"Claude no respondio esta consulta: {respuesta.StopDetails?.Explanation ?? "rechazada por politicas de uso"}.");

            var texto = string.Concat(respuesta.Content
                .Select(b => b.Value)
                .OfType<BetaTextBlock>()
                .Select(t => t.Text)).Trim();

            if (string.IsNullOrWhiteSpace(texto))
                throw new InvalidOperationException(
                    $"Claude no devolvio texto (motivo: {respuesta.StopReason?.ToString() ?? "desconocido"}).");

            var parametrosJson = JsonSerializer.Serialize(new
            {
                model = _settings.Model,
                fallbackModel = _settings.FallbackModel,
                maxTokens = _settings.MaxTokens,
                system = _settings.SystemInstruction,
            });

            return new ClaudeResultado(
                texto,
                respuesta.Model.ToString(),
                (int)(respuesta.Usage.InputTokens + respuesta.Usage.OutputTokens),
                parametrosJson);
        }
    }
}
