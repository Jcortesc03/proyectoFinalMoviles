using System.Reflection;
using MauiApp1.Configuration;
using Microsoft.Extensions.Configuration;

namespace MauiApp1.Services
{
    /// <summary>
    /// Carga y expone la configuracion de la aplicacion desde el recurso embebido
    /// <c>Configuration/appsettings.json</c>, sobreescrita por las variables de entorno
    /// definidas en <see cref="EnvironmentVariables"/>.
    /// </summary>
    /// <remarks>
    /// Se registra como singleton en <see cref="MauiProgram"/>. Orden de prioridad (de menor a mayor):
    /// <c>appsettings.json</c> &lt; <c>Configuration/.env</c> &lt; variables de entorno del proceso.
    /// </remarks>
    public sealed class AppConfigurationService
    {
        private const string EmbeddedResourceName = "MauiApp1.Configuration.appsettings.json";

        /// <summary>Configuracion completa, deserializada desde appsettings.json y variables de entorno.</summary>
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
                .AddInMemoryCollection(EnvironmentVariables.Load())
                .Build();

            Settings = configuration.Get<AppSettings>()
                ?? new AppSettings();
        }
    }
}
