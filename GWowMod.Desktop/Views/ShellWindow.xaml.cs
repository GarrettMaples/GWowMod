using System;
using System.Windows.Controls;
using GWowMod.Desktop.Contracts.Views;
using GWowMod.Desktop.ViewModels;
using MahApps.Metro.Controls;

namespace GWowMod.Desktop.Views
{
    public partial class ShellWindow : MetroWindow, IShellWindow
    {
        private readonly ShellViewModel _viewModel;

        public ShellWindow(ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            WowInstalls.ItemsSource = viewModel.Source;
            _viewModel = viewModel;

            viewModel.LoadingComplete += ViewModel_OnLoadingComplete;
        }

        private void ViewModel_OnLoadingComplete(object sender, EventArgs e)
        {
            WowInstalls.SelectedIndex = _viewModel.InstallPathIndex;
        }

        protected override void OnActivated(EventArgs e)
        {
        }

        public Frame GetNavigationFrame()
            => shellFrame;

        public Frame GetRightPaneFrame()
            => rightPaneFrame;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        public SplitView GetSplitView()
            => splitView;

        private void WowInstalls_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.InstallPathIndex = ((ComboBox) sender).SelectedIndex;
        }
    }
}
