﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GWowMod.Desktop.Contracts.Services;
using GWowMod.Desktop.Helpers;

namespace GWowMod.Desktop.ViewModels
{
    // You can show pages in different ways (update main view, navigate, right pane, new windows or dialog)
    // using the NavigationService, RightPaneService and WindowManagerService.
    // Read more about MenuBar project type here:
    // https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/WPF/projectTypes/menubar.md
    public class ShellViewModel : Observable
    {
        private readonly INavigationService _navigationService;
        private readonly IRightPaneService _rightPaneService;

        private readonly IWowPathProvider _wowPathProvider;

        private RelayCommand _goBackCommand;
        private ICommand _menuViewsDataGridCommand;
        private ICommand _menuViewsMainCommand;
        private ICommand _menuFileExitCommand;
        private ICommand _loadedCommand;
        private ICommand _unloadedCommand;

        private bool _initialized;
        private int _installPathIndex;

        public RelayCommand GoBackCommand => _goBackCommand ??= new RelayCommand(OnGoBack, CanGoBack);

        public ICommand MenuFileExitCommand => _menuFileExitCommand ??= new RelayCommand(OnMenuFileExit);

        public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(OnLoaded);

        public ICommand UnloadedCommand => _unloadedCommand ??= new RelayCommand(OnUnloaded);

        public ObservableCollection<string> Source { get; } = new ObservableCollection<string>();

        public int InstallPathIndex
        {
            get => _installPathIndex;
            set
            {
                _wowPathProvider.InstallPathIndex = value;
                _installPathIndex = value;
            }
        }

        public ShellViewModel(INavigationService navigationService, IRightPaneService rightPaneService, IWowPathProvider wowPathProvider)
        {
            _navigationService = navigationService;
            _rightPaneService = rightPaneService;
            _wowPathProvider = wowPathProvider;
        }

        public virtual event EventHandler Loading;

        public virtual event EventHandler LoadingComplete;

        private async void OnLoaded()
        {
            Loading?.Invoke(this, EventArgs.Empty);

            _navigationService.Navigated += OnNavigated;

            if (_initialized)
            {
                return;
            }

            foreach (var wowInstallPath in await _wowPathProvider.GetInstallPaths())
            {
                Source.Add(wowInstallPath);
            }

            _initialized = true;

            LoadingComplete?.Invoke(this, EventArgs.Empty);
        }

        private void OnUnloaded()
        {
            _rightPaneService.CleanUp();
            _navigationService.Navigated -= OnNavigated;
        }

        private bool CanGoBack()
            => _navigationService.CanGoBack;

        private void OnGoBack()
            => _navigationService.GoBack();

        private void OnNavigated(object sender, string viewModelName)
        {
            GoBackCommand.OnCanExecuteChanged();
        }

        private void OnMenuFileExit()
            => Application.Current.Shutdown();

        public ICommand MenuViewsMainCommand => _menuViewsMainCommand ??= new RelayCommand(OnMenuViewsMain);

        private void OnMenuViewsMain()
            => _navigationService.NavigateTo(typeof(MainViewModel).FullName, null, true);

        public ICommand MenuViewsDataGridCommand => _menuViewsDataGridCommand ??= new RelayCommand(OnMenuViewsDataGrid);

        private void OnMenuViewsDataGrid()
            => _navigationService.NavigateTo(typeof(InstalledAddonsViewModel).FullName, null, true);
    }
}