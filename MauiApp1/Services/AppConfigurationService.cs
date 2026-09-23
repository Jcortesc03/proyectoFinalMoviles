using System.Reflection;
using MauiApp1.Configuration;
using Microsoft.Extensions.Configuration;

namespace MauiApp1.Services
{
    /// <summary>
    /// Carga y expone la configuracion de la aplicacion desde el recurso embebido
    /// <c>Configuration/appsettings.json</c>.
    /// </summary>
    /// <remarks>
    /// Se registra como singleton en <see cref="MauiProgram"/>. Los valores son de solo lectura
    /// en tiempo de ejecucion; para secretos como <see cref="AiApiSettings.ApiKey"/>,
    /// se recomienda usar <c>SecureStorage</c> o variables de entorno y sobreescribir las
    /// propiedades tras la construccion.
    /// </remarks>
    public sealed class AppConfigurationService
    {
        private const string EmbeddedResourceName = "MauiApp1.Configuration.appsettings.json";

        /// <summary>Configuracion completa, deserializada desde appsettings.json.</summary>
        public AppSettings Settings { get; }

        public AppConfigurationService()
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(EmbeddedResourceName)
                ?? throw new InvalidOperationException(
                    $"No se encontro el recurso embebido '{EmbeddedResourceName}'. " +
                    "Verifica que Configuration/appsettings.json este declarado como EmbeddedResource en el .csproj.");

            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            Settings = configuration.Get<AppSettings>()
                ?? new AppSettings();
        }
    }
}