using System;

using GWowMod.UI.ViewModels;

using Windows.UI.Xaml.Controls;

namespace GWowMod.UI.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
