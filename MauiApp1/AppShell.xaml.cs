using MauiApp1.Views;

namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Ruta de la ventana de Detalle (se abre desde el historial o desde Inicio).
            Routing.RegisterRoute(nameof(DetallePage), typeof(DetallePage));
        }
    }
}