using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class HistorialPage : ContentPage
    {
        public HistorialPage()
        {
            InitializeComponent();
            BindingContext = ServiceHelper.GetService<HistorialViewModel>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = ((HistorialViewModel)BindingContext).CargarAsync();
        }
    }
}