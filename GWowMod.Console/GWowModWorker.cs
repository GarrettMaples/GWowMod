using GWowMod.Requests;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace GWowMod.Console
{
    public interface IGWowModWorker
    {
        Task Run(Program.CliOptions cliOptions);
    }

    internal class GWowModWorker : IGWowModWorker
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GWowModWorker> _logger;
        private readonly IWowPathProvider _wowPathProvider;

        public GWowModWorker(IMediator mediator, ILogger<GWowModWorker> logger, IWowPathProvider wowPathProvider)
        {
            _mediator = mediator;
            _logger = logger;
            _wowPathProvider = wowPathProvider;
        }

        public async Task Run(Program.CliOptions cliOptions)
        {
            await RunOptions(cliOptions);
        }

        private async Task RunOptions(Program.CliOptions cliOptions)
        {
            if (!string.IsNullOrWhiteSpace(cliOptions.InstallPath))
            {
                await _wowPathProvider.SaveInstallPath(cliOptions.InstallPath);
                _logger.LogInformation($"Set Wow Install Path: {cliOptions.InstallPath}");
            }

            if (cliOptions.Config)
            {
                var installPath = await _wowPathProvider.GetInstallPath();

                if (string.IsNullOrWhiteSpace(installPath))
                {
                    _logger.LogInformation("WoW Install Path not set");
                }
                else
                {
                    _logger.LogInformation($"WoW Install Path: {installPath}");
                }
            }

            if (cliOptions.UpdateAddons)
            {
                var updateAllAddonsRequest = new UpdateAllAddonsRequest();
                await _mediator.Send(updateAllAddonsRequest);
            }

            if (cliOptions.Addons)
            {
                var addonsRequest = new AddonsRequest();
                var matchingGamesPayload = await _mediator.Send(addonsRequest);

                foreach (var exactMatch in matchingGamesPayload.exactMatches)
                {
                    _logger.LogInformation($"Id: {exactMatch.File.Id} Name: {exactMatch.File.modules[0].foldername} " +
                        $"Version: {exactMatch.File.FileName} File Date: {exactMatch.File.FileDate}");
                }
            }

            if (cliOptions.UpdateAddon.HasValue)
            {
                var addonsRequest = new AddonsRequest();
                var matchingGamesPayload = await _mediator.Send(addonsRequest);

                var exactMatch = matchingGamesPayload.exactMatches.FirstOrDefault(x => x.Id == cliOptions.UpdateAddon.Value);
                if (exactMatch == null)
                {
                    _logger.LogInformation($"Unable to find addon with Id: {cliOptions.UpdateAddon.Value}");
                }
                else
                {
                    var updateAddonRequest = new UpdateAddonRequest();
                    await _mediator.Send(updateAddonRequest);
                }
            }
        }
    }
}