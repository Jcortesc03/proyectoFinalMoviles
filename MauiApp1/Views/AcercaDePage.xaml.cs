using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class AcercaDePage : ContentPage
    {
        public AcercaDePage()
        {
            InitializeComponent();
            BindingContext = ServiceHelper.GetService<AcercaDeViewModel>();
        }
    }
}