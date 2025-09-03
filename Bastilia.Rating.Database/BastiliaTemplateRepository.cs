
namespace Bastilia.Rating.Database
{
    internal class BastiliaTemplateRepository(AppDbContext context) : BastiliaRepositoryBase, IBastiliaTemplateRepository
    {
        public async Task<IReadOnlyCollection<AchievementTemplate>> GetAchievementTemplates()
        {
            var result = await context.AchievementTemplates.Include(at => at.Project).ToListAsync();
            return [.. result.Select(x => new AchievementTemplate(ToProjectLink(x.Project), x.AchievementName, x.AchievementDescription,
                x.AchievementImageUrl is null || x.AchievementImageUrl == "https://bastilia.ru/images/logo-low.jpg", x.AchievementRatingValue, x.YearlyAchievement))];
        }
    }
}