using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1.Services
{
    /// <summary>
    /// Localizador estatico de servicios. Permite acceder al contenedor de DI desde
    /// clases que no pueden inyectar dependencias por constructor (como las paginas
    /// creadas mediante <c>DataTemplate</c> en <c>Shell</c>).
    /// </summary>
    public static class ServiceHelper
    {
        private static IServiceProvider? _services;

        /// <summary>Proveedor de servicios del contenedor de DI de la aplicacion.</summary>
        public static IServiceProvider Services =>
            _services ?? throw new InvalidOperationException(
                "El proveedor de servicios no ha sido inicializado. Llama a Initialize() en el arranque de la aplicacion.");

        /// <summary>Inicializa el localizador con el contenedor de la aplicacion (una sola vez).</summary>
        public static void Initialize(IServiceProvider services) => _services = services;

        /// <summary>Obtiene un servicio registrado en el contenedor de DI.</summary>
        public static T GetService<T>() where T : notnull =>
            Services.GetRequiredService<T>();
    }
}