using System.Collections.Generic;
using GWowMod.JSON;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Requests
{
    public class AddonsRequest : IRequest<IAsyncEnumerable<Match>>
    {
        public AddonsRequest(string installPath)
        {
            InstallPath = installPath;
        }

        public string InstallPath { get; }
    }

    internal class AddonsRequestHandler : RequestHandler<AddonsRequest, IAsyncEnumerable<Match>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddonsRequestHandler> _logger;
        private readonly ICurseForgeClient _curseForgeClient;

        public AddonsRequestHandler(IMediator mediator, ILogger<AddonsRequestHandler> logger, ICurseForgeClient curseForgeClient)
        {
            _mediator = mediator;
            _logger = logger;
            _curseForgeClient = curseForgeClient;
        }

        protected override async IAsyncEnumerable<Match> Handle(AddonsRequest request)
        {
            var fingerPrintRequest = new FingerPrintRequest(request.InstallPath);
            var fingerPrints = (await _mediator.Send(fingerPrintRequest)).ToArray();

            _logger.LogInformation($"FingerPrint count: {fingerPrints.Length}");

            if (!fingerPrints.Any())
            {
                yield break;
            }

            var result = await _curseForgeClient.GetMatchingGames(fingerPrints);

            foreach (var r in result.exactMatches)
            {
                yield return new Match() {File = r.File, Id = r.Id, LatestFile = r.LatestFile};
            }

            //Have to do this because API will return fuzzy matches regardless of how they were installed
            foreach (var fuzzyMatch in ProcessFuzzyMatches(result))
            {
                yield return fuzzyMatch;
            }
        }

        private IEnumerable<Match> ProcessFuzzyMatches(MatchingGamesPayload result)
        {
            foreach (var r in result.partialMatches)
            {
                if (r.File.Modules.All(x => result.unmatchedFingerprints.Contains(x.Fingerprint)))
                {
                    yield return new Match() { File = r.File, Id = r.Id, LatestFile = r.LatestFile };
                }
                else
                {
                    foreach (var l in r.LatestFiles)
                    {
                        if (l.Modules.All(x => result.unmatchedFingerprints.Contains(x.Fingerprint)))
                        {
                            yield return new Match() { File = l, Id = l.Id, LatestFile = r.LatestFile };
                        }
                    }
                }
            }
        }
    }
}