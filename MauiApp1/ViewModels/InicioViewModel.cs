using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiApp1.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana <c>Inicio</c>: presenta el proposito del proyecto,
    /// los creadores y el acceso rapido al resto de ventanas.
    /// </summary>
    public partial class InicioViewModel : ObservableObject
    {
        /// <summary>Descripcion del proposito del proyecto.</summary>
        public string Proposito =>
            "Este proyecto es un buscador sencillo que se conecta a una API de inteligencia " +
            "artificial externa para responder preguntas. Se comporta como un navegador que " +
            "devuelve solo texto plano, guardando cada consulta y su respuesta en una base de " +
            "datos local para poder consultarlas despues.";

        /// <summary>Nombre del proyecto.</summary>
        public string NombreProyecto => "Buscador IA";

        /// <summary>Integrantes del equipo de desarrollo.</summary>
        public IReadOnlyList<string> Integrantes { get; } =
            new[] { "Juan Camilo Cortes", "Juan Camilo Ibarra", "Isaac Moreno", "Andruw Sarrias" };

        [RelayCommand]
        private Task IrAConsultarAsync() =>
            Shell.Current.GoToAsync("//ConsultarIaPage");

        [RelayCommand]
        private Task IrAHistorialAsync() =>
            Shell.Current.GoToAsync("//HistorialPage");

        [RelayCommand]
        private Task IrADetalleAsync() =>
            Shell.Current.GoToAsync(nameof(MauiApp1.Views.DetallePage));

        [RelayCommand]
        private Task IrAAcercaDeAsync() =>
            Shell.Current.GoToAsync("//AcercaDePage");
    }
}