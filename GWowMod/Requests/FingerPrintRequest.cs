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
        public FingerPrintRequest(string installPath)
        {
            InstallPath = installPath;
        }

        public string InstallPath { get; }
    }

    internal class FingerPrintRequestHandler : IRequestHandler<FingerPrintRequest, IEnumerable<long>>
    {
        private readonly ILogger<UpdateAddonRequestHandler> _logger;
        private readonly IFingerPrintScanner _fingerPrintScanner;
        private readonly ICurseForgeClient _curseForgeClient;

        public FingerPrintRequestHandler
        (
            ILogger<UpdateAddonRequestHandler> logger,
            IFingerPrintScanner fingerPrintScanner,
            ICurseForgeClient curseForgeClient
        )
        {
            _logger = logger;
            _fingerPrintScanner = fingerPrintScanner;
            _curseForgeClient = curseForgeClient;
        }

        public async Task<IEnumerable<long>> Handle(FingerPrintRequest request, CancellationToken cancellationToken)
        {
            var installPath = request.InstallPath;

            if (string.IsNullOrWhiteSpace(installPath))
            {
                throw new InvalidOperationException("Unable to update addons - WoW Install Path not set");
            }

            _logger.LogInformation($"Updating addons in {installPath}...");

            var game = await _curseForgeClient.GetGame();
            var section = game.categorySections.FirstOrDefault();

            if (section == null)
            {
                throw new InvalidOperationException("Invalid number of sections found");
            }

            section.SetDirectory(installPath);

            return _fingerPrintScanner.GetFingerPrints(game);
        }
    }
}