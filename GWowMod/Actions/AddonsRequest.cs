using GWowMod.JSON;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Actions
{
    public class AddonsRequest : IRequest<MatchingGamesPayload>
    {
    }

    internal class AddonsRequestHandler : IRequestHandler<AddonsRequest, MatchingGamesPayload>
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

        public async Task<MatchingGamesPayload> Handle(AddonsRequest request, CancellationToken cancellationToken)
        {
            var fingerPrintRequest = new FingerPrintRequest();
            long[] fingerPrints = (await _mediator.Send(fingerPrintRequest, cancellationToken)).ToArray();

            _logger.LogInformation($"FingerPrint count: {fingerPrints.Length}");

            if (!fingerPrints.Any())
            {
                return null;
            }

            return await _curseForgeClient.GetMatchingGames(fingerPrints);
        }
    }
}