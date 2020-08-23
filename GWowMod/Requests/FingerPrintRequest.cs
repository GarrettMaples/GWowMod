using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Requests
{
    public class FingerPrintRequest : IRequest<IEnumerable<long>>
    {
    }

    internal class FingerPrintRequestHandler : IRequestHandler<FingerPrintRequest, IEnumerable<long>>
    {
        private readonly IWowPathProvider _wowPathProvider;
        private readonly ILogger<UpdateAddonRequestHander> _logger;
        private readonly IFingerPrintScanner _fingerPrintScanner;
        private readonly ICurseForgeClient _curseForgeClient;

        public FingerPrintRequestHandler
        (
            IWowPathProvider wowPathProvider,
            ILogger<UpdateAddonRequestHander> logger,
            IFingerPrintScanner fingerPrintScanner,
            ICurseForgeClient curseForgeClient
        )
        {
            _wowPathProvider = wowPathProvider;
            _logger = logger;
            _fingerPrintScanner = fingerPrintScanner;
            _curseForgeClient = curseForgeClient;
        }

        public async Task<IEnumerable<long>> Handle(FingerPrintRequest request, CancellationToken cancellationToken)
        {
            string installPath = await _wowPathProvider.GetInstallPath();

            if (string.IsNullOrWhiteSpace(installPath))
            {
                throw new InvalidOperationException("Unable to update addons - WoW Install Path not set");
            }

            _logger.LogInformation($"Updating addons in {installPath}...");

            Game game = await _curseForgeClient.GetGame();
            CategorySection section = game.categorySections.FirstOrDefault();

            if (section == null)
            {
                throw new InvalidOperationException("Invalid number of sections found");
            }

            section.SetDirectory(installPath);

            return _fingerPrintScanner.GetFingerPrints(game);
        }
    }
}