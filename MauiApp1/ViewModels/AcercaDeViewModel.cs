using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel;

namespace MauiApp1.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana <c>Acerca de</c>: detalles del proyecto.
    /// </summary>
    public partial class AcercaDeViewModel : ObservableObject
    {
        /// <summary>Nombre del proyecto.</summary>
        public string NombreProyecto => "Buscador IA";

        /// <summary>Version de la aplicacion.</summary>
        public string Version => AppInfo.Current.VersionString;

        /// <summary>Descripcion general del proyecto.</summary>
        public string Descripcion =>
            "Buscador IA es una aplicacion multiplataforma (.NET MAUI) que actua como un " +
            "navegador simple hacia una API de inteligencia artificial externa: recibe " +
            "preguntas y muestra la respuesta en texto plano. Lleva un historial local de " +
            "cada consulta, con su detalle tecnico, y permite exportarlo a PDF.";

        /// <summary>Tecnologias utilizadas.</summary>
        public string Tecnologias =>
            ".NET MAUI 10, C#, API de Claude (SDK Anthropic), SQLite (sqlite-net-pcl), CommunityToolkit.Maui y QuestPDF.";

        /// <summary>Integrantes del equipo.</summary>
        public IReadOnlyList<string> Integrantes { get; } =
            new[] { "Juan Camilo Cortes", "Juan Camilo Ibarra", "Isaac Moreno", "Andruw Sarrias" };
    }
}