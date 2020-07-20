using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Actions
{
    public class FingerPrintRequest : IRequest<IEnumerable<long>>
    {
        
    }
    
    internal class FingerPrintRequestHandler : IRequestHandler<FingerPrintRequest, IEnumerable<long>>
    {
        private readonly IWowPathProvider _wowPathProvider;
        private readonly ILogger<UpdateAddonRequestHander> _logger;
        private readonly IFingerPrintScanner _fingerPrintScanner;

        public FingerPrintRequestHandler(IWowPathProvider wowPathProvider, ILogger<UpdateAddonRequestHander> logger, IFingerPrintScanner fingerPrintScanner)
        {
            _wowPathProvider = wowPathProvider;
            _logger = logger;
            _fingerPrintScanner = fingerPrintScanner;
        }

        public async Task<IEnumerable<long>> Handle(FingerPrintRequest request, CancellationToken cancellationToken)
        {
            string installPath = await _wowPathProvider.GetInstallPath();
            
            if (string.IsNullOrWhiteSpace(installPath))
            {
                throw new InvalidOperationException("Unable to update addons - WoW Install Path not set");
            }

            _logger.LogInformation($"Updating addons in {installPath}...");

            var game = JsonSerializer.Deserialize<Game>(await System.IO.File.ReadAllTextAsync(@"C:\Users\garre\source\repos\GWowMod\Configs\Wow Game Instance.json"));
            CategorySection section = game.categorySections.FirstOrDefault();
            section.SetDirectory(installPath);

            if (section == null)
            {
                throw new InvalidOperationException("Invalid number of sections found");
            }

            return _fingerPrintScanner.GetFingerPrints(game);
        }
    }
}