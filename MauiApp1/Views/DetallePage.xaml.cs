using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    /// <summary>
    /// Página de detalle de una consulta. Recibe el identificador de la consulta
    /// mediante la query <c>?id=X</c> de la navegación Shell.
    /// </summary>
    [QueryProperty(nameof(ConsultaId), "id")]
    public partial class DetallePage : ContentPage
    {
        public DetallePage()
        {
            InitializeComponent();
            BindingContext = ServiceHelper.GetService<DetalleViewModel>();
        }

        /// <summary>
        /// Identificador de la consulta a mostrar. Al cambiar (vía navegación Shell)
        /// se recarga el detalle en el ViewModel.
        /// </summary>
        /// <remarks>
        /// No llamarla <c>Id</c>: ocultaria <c>Element.Id</c> (Guid) y Shell, que asigna la
        /// query por reflexion, fallaria con <c>AmbiguousMatchException</c>.
        /// </remarks>
        public int ConsultaId
        {
            set => _ = ((DetalleViewModel)BindingContext).CargarAsync(value);
        }
    }
}