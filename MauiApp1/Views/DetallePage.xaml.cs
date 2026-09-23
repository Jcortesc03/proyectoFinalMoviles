using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    /// <summary>
    /// Página de detalle de una consulta. Recibe el identificador de la consulta
    /// mediante la query <c>?id=X</c> de la navegación Shell.
    /// </summary>
    [QueryProperty(nameof(Id), "id")]
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
        public new int Id
        {
            set => _ = ((DetalleViewModel)BindingContext).CargarAsync(value);
        }
    }
}