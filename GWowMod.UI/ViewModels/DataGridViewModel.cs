using GWowMod.Requests;
using GWowMod.UI.Core.Models;
using GWowMod.UI.Core.Services;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GWowMod.UI.ViewModels
{
    public class DataGridViewModel : ViewModelBase
    {
        private readonly ISampleDataService _sampleDataService;

        public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

        public DataGridViewModel(ISampleDataService sampleDataServiceInstance)
        {
            _sampleDataService = sampleDataServiceInstance;
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            Source.Clear();

            var addonsRequest = new AddonsRequest();
            var matchingGamesPayload = await _mediator.Send(addonsRequest);

            foreach (var exactMatch in matchingGamesPayload.exactMatches)
            {
                _logger.LogInformation($"Id: {exactMatch.file.id} Name: {exactMatch.file.modules[0].foldername} " +
                    $"Version: {exactMatch.file.fileName} File Date: {exactMatch.file.fileDate}");
            }

            // // TODO WTS: Replace this with your actual data
            // var data = await _sampleDataService.GetGridDataAsync();
            //
            // foreach (var item in data)
            // {
            //     Source.Add(item);
            // }
        }
    }
}
