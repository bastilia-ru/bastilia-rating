
namespace Bastilia.Rating.Database
{
    internal class BastiliaTemplateRepository(IDbContextFactory<AppDbContext> contextFactory) : BastiliaRepositoryBase, IBastiliaTemplateRepository
    {
        public async Task<IReadOnlyCollection<AchievementTemplate>> GetAchievementTemplates()
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var result = await context.AchievementTemplates.Include(at => at.Project).ToListAsync();
            return [.. result.Select(ToTemplate)];
        }
    }
}
