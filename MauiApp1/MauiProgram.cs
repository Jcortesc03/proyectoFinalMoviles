using CommunityToolkit.Maui;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using Microsoft.Extensions.Logging;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("ArchivoBlack-Regular.ttf", "RisoDisplay");
                    fonts.AddFont("SpaceMono-Regular.ttf", "RisoMono");
                    fonts.AddFont("SpaceMono-Bold.ttf", "RisoMonoBold");
                });

            // Configuracion y acceso a datos (SQLite)
            builder.Services.AddSingleton<AppConfigurationService>();
            builder.Services.AddSingleton<DatabaseService>();

            // Servicios de aplicacion
            builder.Services.AddSingleton<ClaudeApiService>();
            builder.Services.AddSingleton<PdfExportService>();

            // ViewModels
            builder.Services.AddSingleton<InicioViewModel>();
            builder.Services.AddSingleton<ConsultarIaViewModel>();
            builder.Services.AddSingleton<HistorialViewModel>();
            builder.Services.AddSingleton<DetalleViewModel>();
            builder.Services.AddSingleton<AcercaDeViewModel>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            ServiceHelper.Initialize(app.Services);

            return app;
        }
    }
}