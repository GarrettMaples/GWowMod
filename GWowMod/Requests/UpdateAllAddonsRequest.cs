using GWowMod.JSON;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Requests
{
    public class UpdateAllAddonsRequest : IRequest
    {
    }

    internal class UpdateAllAddonsRequestHandler : IRequestHandler<UpdateAllAddonsRequest>
    {
        private readonly ICurseForgeClient _curseForgeClient;
        private readonly ILogger<UpdateAddonRequestHandler> _logger;
        private readonly IMediator _mediator;

        public UpdateAllAddonsRequestHandler(ICurseForgeClient curseForgeClient, ILogger<UpdateAddonRequestHandler> logger, IMediator mediator)
        {
            _curseForgeClient = curseForgeClient;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UpdateAllAddonsRequest request, CancellationToken cancellationToken)
        {
            var addonsRequest = new AddonsRequest();
            MatchingGamesPayload matchingGamesPayload = await _mediator.Send(addonsRequest, cancellationToken);

            var updateAddonRequest = new UpdateAddonRequest(matchingGamesPayload.exactMatches.ToArray());
            return await _mediator.Send(updateAddonRequest, cancellationToken);
        }
    }
}