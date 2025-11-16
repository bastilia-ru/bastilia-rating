using Bastilia.Rating.Database.Entities;

namespace Bastilia.Rating.Database.DbServices
{
    internal class AchievementService(AppDbContext appDbContext) : IAchievementService
    {
        public async Task GrantAchivement(int projectId, int userId, int templateId, int grantedById, string? overrideName)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .Include(x => x.AchievementTemplates).ThenInclude(x => x.Achievements)
                .Include(x => x.ProjectAdmins).ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            var grantDate = entity.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);


            var template = entity.AchievementTemplates.First(x => x.AchievementTemplateId == templateId);

            var expirationDate = grantDate.AddYears(template.YearlyAchievement ? 1 : 2);

            var achivement = new Achievement()
            {
                AchievementTemplateId = templateId,
                ExpirationDate = expirationDate,
                GrantedByUser = null!,
                GrantedBy = grantedById,
                RemovedByUser = null,
                RemovedBy = null,
                GrantedDate = grantDate,
                Template = null!,
                User = null!,
                UserId = userId,
                OverrideName = overrideName,
                RemovedDate = null
            };

            await appDbContext.AddAsync(achivement);

            await appDbContext.SaveChangesAsync();
        }
    }
}
