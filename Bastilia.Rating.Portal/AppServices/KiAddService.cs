using Bastilia.Rating.Domain;
using JoinRpg.Common.KogdaIgraClient;

namespace Bastilia.Rating.Portal.AppServices
{
    public class KiAddService(IKogdaIgraApiClient kogdaIgraApiClient, IKiDbService kiDbService)
    {
        public async Task<bool> AddKogdaIgraGame(int kogdaIgraId)
        {
            var game = await kogdaIgraApiClient.GetGameInfo(kogdaIgraId);
            if (game is null)
            {
                return false;
            }
            await kiDbService.AddKogdaIgraGame(game.Id, game.Name, game.Begin, game.End, game.UpdateDate);
            return true;
        }
    }
}
