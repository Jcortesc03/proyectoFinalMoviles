using MauiApp1.Services;

namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Inicializa la base de datos SQLite al arrancar (crea tablas si no existen).
            var databaseService = ServiceHelper.GetService<DatabaseService>();
            _ = databaseService.InitializeAsync();

            return new Window(new AppShell());
        }
    }
}