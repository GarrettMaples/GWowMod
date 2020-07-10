using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GWowMod
{
    public interface ICurseForgeClient
    {
        [Post("/v2/fingerprint")]
        Task<MatchingGamesPayload> GetMatchingGames(long[] fingerPrints);
    }
}