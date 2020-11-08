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

        private bool _initialized;
        private ICommand _refreshInstalledAddonsCommand;
        private ICommand _updateAddonCommand;
        private readonly IMapper _mapper;

        public ICommand RefreshInstalledAddonsCommand =>
            _refreshInstalledAddonsCommand ??= new RelayCommand(async () => await LoadInstalledAddons());

        public ICommand UpdateAddonCommand => _updateAddonCommand ??= new RelayCommand<int>(async (x) => await UpdateAddon(x));
        public ObservableCollection<InstalledAddonModel> Source { get; } = new ObservableCollection<InstalledAddonModel>();

        public InstalledAddonsViewModel(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async void OnNavigatedTo(object parameter)
        {
            if (_initialized)
            {
                return;
            }

            await LoadInstalledAddons();
            _initialized = true;
        }

        public async Task LoadInstalledAddons()
        {
            OnLoading();

            Source.Clear();

            var addonsResult = await Task.Run(async () =>
            {
                var addonsRequest = new AddonsRequest();
                var matchingGamesPayload = await _mediator.Send(addonsRequest);

                return matchingGamesPayload;
            });

            foreach (var match in addonsResult.exactMatches)
            {
                var addon = _mapper.Map<ExactMatch, InstalledAddonModel>(match);
                Source.Add(addon);
            }

            OnLoadingComplete();
        }

        public async Task UpdateAddon(int id)
        {
            var addonsRequest = new AddonsRequest();
            var matchingGamesPayload = await _mediator.Send(addonsRequest);

            var exactMatch = matchingGamesPayload.exactMatches.FirstOrDefault(x => x.Id == id);
            if (exactMatch == null)
            {
                throw new InvalidOperationException($"Unable to find addon with Id: {id}");
            }

            var updateAddonRequest = new UpdateAddonRequest(exactMatch);
            await _mediator.Send(updateAddonRequest);

            await LoadInstalledAddons();
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