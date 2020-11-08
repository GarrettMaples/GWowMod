using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using GWowMod.Desktop.Contracts.ViewModels;
using GWowMod.Desktop.Helpers;

namespace GWowMod.Desktop.ViewModels
{
    public class WowInstallsViewModel : Observable, INavigationAware
    {
        private readonly IWowPathProvider _wowPathProvider;
        private bool _initialized;

        public WowInstallsViewModel(IWowPathProvider wowPathProvider)
        {
            _wowPathProvider = wowPathProvider;
        }

        public ObservableCollection<string> Source { get; } = new ObservableCollection<string>();

        public async void OnNavigatedTo(object parameter)
        {
            if (_initialized)
            {
                return;
            }

            foreach (var wowInstallPath in await _wowPathProvider.GetInstallPaths())
            {
                Source.Add(wowInstallPath);
            }

            _initialized = true;
        }

        public void OnNavigatedFrom()
        {
        }
    }
}
