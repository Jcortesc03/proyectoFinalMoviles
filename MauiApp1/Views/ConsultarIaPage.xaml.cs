using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class ConsultarIaPage : ContentPage
    {
        public ConsultarIaPage()
        {
            InitializeComponent();
            BindingContext = ServiceHelper.GetService<ConsultarIaViewModel>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((ConsultarIaViewModel)BindingContext).MensajeError = null;
        }
    }
}