﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GWowMod.Desktop.Contracts.Services;
using GWowMod.Desktop.Contracts.Views;
using GWowMod.Desktop.ViewModels;

using Microsoft.Extensions.Hosting;

namespace GWowMod.Desktop.Services
{
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private readonly IRightPaneService _rightPaneService;
        private IShellWindow _shellWindow;

        public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService, IRightPaneService rightPaneService)
        {
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            _rightPaneService = rightPaneService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Initialize services that you need before app activation
            await InitializeAsync();

            await HandleActivationAsync();

            // Tasks after activation
            await StartupAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        private async Task StartupAsync()
        {
            await Task.CompletedTask;
        }

        private async Task HandleActivationAsync()
        {
            if (!Application.Current.Windows.OfType<IShellWindow>().Any())
            {
                // Default activation that navigates to the apps default page
                _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow ?? throw new InvalidOperationException($"Unable to retrieve instance of IShellWindow.");
                _navigationService.Initialize(_shellWindow.GetNavigationFrame());
                _rightPaneService.Initialize(_shellWindow.GetRightPaneFrame(), _shellWindow.GetSplitView());
                _shellWindow.ShowWindow();
                _navigationService.NavigateTo(typeof(InstalledAddonsViewModel).FullName);
                await Task.CompletedTask;
            }
        }
    }
}
