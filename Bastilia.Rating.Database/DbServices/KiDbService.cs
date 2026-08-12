
namespace Bastilia.Rating.Database.DbServices
{
    internal class KiDbService(IDbContextFactory<AppDbContext> contextFactory) : IKiDbService
    {
        public async Task AddKogdaIgraGame(int kogdaIgraId, string name, DateOnly begin, DateOnly end, DateTimeOffset lastUpdatedAt)
        {
            await using var appDbContext = await contextFactory.CreateDbContextAsync();
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