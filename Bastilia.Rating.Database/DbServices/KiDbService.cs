
namespace Bastilia.Rating.Database.DbServices
{
    internal class KiDbService(AppDbContext appDbContext) : IKiDbService
    {
        public async Task AddKogdaIgraGame(int kogdaIgraId, string name, DateOnly begin, DateOnly end, DateTimeOffset lastUpdatedAt)
        {
            var entity = await appDbContext.Set<Entities.KogdaIgraGame>().FindAsync(kogdaIgraId);
            if (entity is null)
            {
                entity = new Entities.KogdaIgraGame()
                {
                    KogdaIgraGameId = kogdaIgraId,
                    EndDate = end,
                    LastUpdatedAt = lastUpdatedAt.ToUniversalTime(),
                    Name = name,
                    StartDate = begin,
                };
                appDbContext.Set<Entities.KogdaIgraGame>().Add(entity);
            }
            else
            {
                entity.EndDate = end;
                entity.StartDate = begin;
                entity.LastUpdatedAt = lastUpdatedAt.ToUniversalTime();
                entity.Name = name;
            }
            await appDbContext.SaveChangesAsync();
        }
    }
}