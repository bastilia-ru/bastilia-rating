
namespace Bastilia.Rating.Database
{
    internal class BastiliaTemplateRepository(AppDbContext context) : BastiliaRepositoryBase, IBastiliaTemplateRepository
    {
        public async Task<IReadOnlyCollection<AchievementTemplate>> GetAchievementTemplates()
        {
            var result = await context.AchievementTemplates.Include(at => at.Project).ToListAsync();
            return [.. result.Select(ToTemplate)];
        }
    }
}