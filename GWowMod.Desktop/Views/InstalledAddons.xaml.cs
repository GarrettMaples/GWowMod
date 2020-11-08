using System;
using System.Windows;
using System.Windows.Controls;
using GWowMod.Desktop.ViewModels;

namespace GWowMod.Desktop.Views
{
    public partial class InstalledAddonsPage : Page
    {
        public InstalledAddonsPage(InstalledAddonsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.Loading += ViewModel_OnLoading;
            viewModel.LoadingComplete += ViewModel_OnLoadingComplete;
        }

        private void ViewModel_OnLoadingComplete(object sender, EventArgs e)
        {
            LoadingIcon.Visibility = Visibility.Hidden;
        }

        private void ViewModel_OnLoading(object sender, EventArgs e)
        {
            LoadingIcon.Visibility = Visibility.Visible;
        }
    }
}
