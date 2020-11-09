using System;
using System.Windows.Controls;
using GWowMod.Desktop.Contracts.Views;
using GWowMod.Desktop.ViewModels;
using MahApps.Metro.Controls;

namespace GWowMod.Desktop.Views
{
    public partial class ShellWindow : MetroWindow, IShellWindow
    {
        public ShellWindow(ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        protected override void OnActivated(EventArgs e)
        {
        }

        public Frame GetNavigationFrame()
            => ShellFrame;

        public Frame GetRightPaneFrame()
            => RightPaneFrame;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        public SplitView GetSplitView()
            => SplitView;
    }
}
