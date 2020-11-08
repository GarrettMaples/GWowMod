using System.Windows.Controls;

using GWowMod.Desktop.ViewModels;

namespace GWowMod.Desktop.Views
{
    public partial class MainPage : Page
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
