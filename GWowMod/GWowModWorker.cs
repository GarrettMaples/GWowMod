using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GWowMod
{
    public interface IGWowModWorker
    {
        Task Run(Program.CliOptions cliOptions);
    }

    internal class GWowModWorker : IGWowModWorker
    {
        private readonly IAddonUpdater _addonUpdater;
        private readonly ILogger<GWowModWorker> _logger;
        private IWowPathProvider _wowPathProvider;

        public GWowModWorker(IAddonUpdater addonUpdater, ILogger<GWowModWorker> logger, IWowPathProvider wowPathProvider)
        {
            _addonUpdater = addonUpdater;
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
            
            if (cliOptions.GetInstallPath)
            {
                var installPath = await _wowPathProvider.GetInstallPath();

                if (string.IsNullOrWhiteSpace(installPath))
                {
                    _logger.LogInformation($"WoW Install Path not set");
                }
                else
                {
                    _logger.LogInformation($"WoW Install Path: {installPath}");   
                }
            }

            if (cliOptions.UpdateAddons)
            {
                await _addonUpdater.UpdateAddons();
            }
        }
    }
}