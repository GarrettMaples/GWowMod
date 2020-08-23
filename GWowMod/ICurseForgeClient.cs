using GWowMod.JSON;
using Refit;
using System.Threading.Tasks;

namespace GWowMod
{
    public interface ICurseForgeClient
    {
        [Post("/v2/fingerprint")]
        Task<MatchingGamesPayload> GetMatchingGames(long[] fingerPrints);

        [Get("/v2/game/1")]
        Task<Game> GetGame();
    }
}