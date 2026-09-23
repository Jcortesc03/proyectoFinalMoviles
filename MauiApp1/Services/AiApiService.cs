using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MauiApp1.Configuration;

namespace MauiApp1.Services
{
    /// <summary>
    /// Cliente HTTP hacia la API de IA externa configurada en <c>appsettings.json</c>.
    /// Envia la consulta del usuario y devuelve la respuesta en texto plano
    /// (el buscador se comporta como un navegador que solo devuelve texto).
    /// </summary>
    /// <remarks>
    /// Peticion enviada (POST <c>AiApi.BaseUrl</c>):
    /// <code>{ "model": "&lt;DefaultModel&gt;", "prompt": "..." }</code>
    /// La respuesta se devuelve tal cual como texto plano. Adaptar
    /// <see cref="AskAsync"/> si la API real usa otro contrato.
    /// </remarks>
    public sealed class AiApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelo;

        public AiApiService(AppConfigurationService configuration)
        {
            var settings = configuration.Settings.AiApi;
            _modelo = settings.DefaultModel;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.BaseUrl),
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
            };

            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        }

        /// <summary>
        /// Envia <paramref name="prompt"/> a la API de IA y devuelve la respuesta en texto plano.
        /// </summary>
        /// <exception cref="HttpRequestException">Si la API no esta disponible o responde con error.</exception>
        public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                model = _modelo,
                prompt,
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(string.Empty, content, cancellationToken)
                .ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"La API de IA respondio con {(int)response.StatusCode}: {body}");

            return body;
        }
    }
}