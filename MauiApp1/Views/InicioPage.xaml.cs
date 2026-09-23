using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class InicioPage : ContentPage
    {
        public InicioPage()
        {
            InitializeComponent();
            BindingContext = ServiceHelper.GetService<InicioViewModel>();
        }
    }
}