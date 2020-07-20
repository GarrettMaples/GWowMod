using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Actions
{
    public class UpdateAllAddonsRequest : IRequest
    {
    }

    internal class UpdateAllAddonsRequestHandler : IRequestHandler<UpdateAllAddonsRequest>
    {
        private readonly ICurseForgeClient _curseForgeClient;
        private readonly ILogger<UpdateAddonRequestHander> _logger;
        private readonly IMediator _mediator;

        public UpdateAllAddonsRequestHandler(ICurseForgeClient curseForgeClient, ILogger<UpdateAddonRequestHander> logger, IMediator mediator)
        {
            _curseForgeClient = curseForgeClient;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UpdateAllAddonsRequest request, CancellationToken cancellationToken)
        {
            var fingerPrintRequest = new FingerPrintRequest();
            List<long> fingerPrints = (await _mediator.Send(fingerPrintRequest)).ToList();

            _logger.LogInformation($"FingerPrint count: {fingerPrints.Count}");

            if (!fingerPrints.Any())
            {
                return Unit.Value;
            }

            MatchingGamesPayload matchingGamesPayload = await _curseForgeClient.GetMatchingGames(fingerPrints.ToArray());

            foreach (ExactMatch exactMatch in matchingGamesPayload.exactMatches)
            {
                var updateAddonRequest = new UpdateAddonRequest(exactMatch);
                await _mediator.Publish(updateAddonRequest, cancellationToken);
            }

            return Unit.Value;
        }
    }
}