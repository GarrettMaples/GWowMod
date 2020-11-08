using System.Windows.Controls;

using GWowMod.Desktop.Contracts.Views;
using GWowMod.Desktop.ViewModels;

using MahApps.Metro.Controls;

namespace GWowMod.Desktop.Views
{
    public partial class ShellDialogWindow : MetroWindow, IShellDialogWindow
    {
        public ShellDialogWindow(ShellDialogViewModel viewModel)
        {
            InitializeComponent();
            viewModel.SetResult = OnSetResult;
            DataContext = viewModel;
        }

        public Frame GetDialogFrame()
            => dialogFrame;

        private void OnSetResult(bool? result)
        {
            DialogResult = result;
            Close();
        }
    }
}
