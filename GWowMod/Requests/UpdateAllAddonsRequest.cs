using System.Linq;
using GWowMod.JSON;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Requests
{
    public class UpdateAllAddonsRequest : IRequest
    {
        public UpdateAllAddonsRequest(string installPath)
        {
            InstallPath = installPath;
        }

        public string InstallPath { get; }
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
            var addonsRequest = new AddonsRequest(request.InstallPath);
            MatchingGamesPayload matchingGamesPayload = await _mediator.Send(addonsRequest, cancellationToken);

            var updateAddonRequest = new UpdateAddonRequest(request.InstallPath, matchingGamesPayload.exactMatches.Where(x => x.LatestFile != null && x.LatestFile.Id != x.Id).ToArray());
            return await _mediator.Send(updateAddonRequest, cancellationToken);
        }
    }
}