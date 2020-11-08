using System;
using System.Windows.Input;

using GWowMod.Desktop.Helpers;

namespace GWowMod.Desktop.ViewModels
{
    public class ShellDialogViewModel : Observable
    {
        private ICommand _closeCommand;

        public ICommand CloseCommand => _closeCommand ??= new RelayCommand(OnClose);

        public Action<bool?> SetResult { get; set; }

        public ShellDialogViewModel()
        {
        }

        private void OnClose()
        {
            var result = true;
            SetResult(result);
        }
    }
}
