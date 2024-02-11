using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GWowMod.Desktop.Contracts.ViewModels;
using GWowMod.Desktop.Helpers;
using GWowMod.JSON;
using GWowMod.Requests;
using MediatR;
using System.Windows.Input;
using AutoMapper;
using GWowMod.Desktop.Models;

namespace GWowMod.Desktop.ViewModels
{
    public class InstalledAddonsViewModel : Observable, INavigationAware
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ShellViewModel _viewModel;

        private ICommand _refreshInstalledAddonsCommand;
        private ICommand _updateAddonCommand;
        private ICommand _updateAllAddonsCommand;

        public ICommand RefreshInstalledAddonsCommand =>
            _refreshInstalledAddonsCommand ??= new RelayCommand(async () => await LoadInstalledAddons(_viewModel.InstallPathValue));

        public ICommand UpdateAddonCommand => _updateAddonCommand ??= new RelayCommand<int>(async (x) => await UpdateAddon(x));

        public ICommand UpdateAllAddonsCommand => _updateAllAddonsCommand ??= new RelayCommand(async () =>
        {
            await Task.Run(async () =>
            {
                var updateAllAddonsRequest = new UpdateAllAddonsRequest(_viewModel.InstallPathValue);
                await _mediator.Send(updateAllAddonsRequest);
            });

            await LoadInstalledAddons(_viewModel.InstallPathValue);
        });

        public ObservableCollection<InstalledAddonModel> Source { get; } = new ObservableCollection<InstalledAddonModel>();

        public InstalledAddonsViewModel(IMediator mediator, IMapper mapper, ShellViewModel viewModel)
        {
            _mediator = mediator;
            _mapper = mapper;
            _viewModel = viewModel;

            viewModel.PropertyChanged += async (sender, args) =>
            {
                var installPathValue = ((ShellViewModel)sender).InstallPathValue;

                if (args.PropertyName != nameof(ShellViewModel.InstallPathValue) || string.IsNullOrWhiteSpace(installPathValue))
                {
                    return;
                }

                await LoadInstalledAddons(installPathValue);
            };
        }

        public void OnNavigatedTo(object parameter)
        {
        }

        public async Task LoadInstalledAddons(string installPath)
        {
            OnLoading();

            Source.Clear();
            
            var addonsResult = await Task.Run(async () =>
            {
                var addonsRequest = new AddonsRequest(installPath);
                var matchingGamesPayload = await _mediator.Send(addonsRequest);

                return matchingGamesPayload;
            });

            await foreach (var match in addonsResult)
            {
                var addon = _mapper.Map<Match, InstalledAddonModel>(match);
                Source.Add(addon);
            }

            OnLoadingComplete();
        }

        public async Task UpdateAddon(int id)
        {
            await Task.Run(async () =>
            {
                var addonsRequest = new AddonsRequest(_viewModel.InstallPathValue);
                var matchingGamesPayload = await _mediator.Send(addonsRequest);

                var exactMatch = await matchingGamesPayload
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (exactMatch == null)
                {
                    throw new InvalidOperationException($"Unable to find addon with Id: {id}");
                }

                var updateAddonRequest = new UpdateAddonRequest(_viewModel.InstallPathValue, exactMatch);
                await _mediator.Send(updateAddonRequest);
            });

            await LoadInstalledAddons(_viewModel.InstallPathValue);
        }

        public virtual event EventHandler Loading;

        public virtual event EventHandler LoadingComplete;

        protected virtual void OnLoading()
        {
            Loading?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLoadingComplete()
        {
            LoadingComplete?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnNavigatedFrom()
        {
        }
    }
}