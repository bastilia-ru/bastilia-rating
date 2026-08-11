using Bastilia.Rating.Database.Entities;
using Bastilia.Rating.Domain.Common;

namespace Bastilia.Rating.Database.DbServices
{
    internal class ProjectService(AppDbContext appDbContext) : IProjectService
    {
        public async Task<int> CreateProject(string projectName, ProjectType projectType, BrandType brandType, bool OngoingProject,
            int? JoinrpgProjectId, int? KogdaIgraProjectId, string ProjectUri, IReadOnlyList<UserIdentification> coordinators,
            DateOnly startDate, DateOnly endDate, bool alreadyCompleted, string projectDescription)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var entity = new Entities.BastiliaProject
            {
                ProjectName = projectName,
                ProjectType = projectType,
                BrandType = brandType,
                OngoingProject = OngoingProject,
                JoinrpgProjectId = JoinrpgProjectId,
                KogdaIgraProjectId = KogdaIgraProjectId,
                ProjectUri = ProjectUri,
                ProjectDescription = projectDescription,
                CreateDate = today,
                PlannedStartDate = startDate,
                PlannedEndDate = endDate,
                StartDate = alreadyCompleted ? startDate : null,
                EndDate = alreadyCompleted ? endDate : null,
                ProjectIconUri = null!,
            };

            foreach (var coordinator in coordinators)
            {
                var user = await appDbContext.Set<Entities.User>().FindAsync(coordinator.Value)
                    ?? throw new InvalidOperationException($"User {coordinator.Value} not found");
                entity.ProjectAdmins.Add(new ProjectAdmin { Project = entity, User = user, AddDate = today });
            }

            await appDbContext.Set<Entities.BastiliaProject>().AddAsync(entity);
            await appDbContext.SaveChangesAsync();

            return entity.BastiliaProjectId;
        }

        public async Task CompleteProject(int projectId, DateOnly endDate, ProjectLevel projectLevel, IReadOnlyList<string> achievementTemplateNames)
        {
            var ratingValues = projectLevel.GetAchievementRatingValues();
            if (achievementTemplateNames.Count != ratingValues.Count)
            {
                throw new ArgumentException(
                    $"Для уровня {projectLevel} требуется {ratingValues.Count} названий ачивок, передано {achievementTemplateNames.Count}",
                    nameof(achievementTemplateNames));
            }

            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            entity.EndDate = endDate;
            entity.OngoingProject = false;

            for (var i = 0; i < achievementTemplateNames.Count; i++)
            {
                appDbContext.Set<Entities.AchievementTemplate>().Add(new Entities.AchievementTemplate
                {
                    Project = entity,
                    Owner = null!,
                    AchievementName = achievementTemplateNames[i],
                    AchievementDescription = "",
                    AchievementImageUrl = "https://bastilia.ru/images/logo-low.jpg",
                    AchievementRatingValue = ratingValues[i],
                    YearlyAchievement = false,
                    CreateDate = DateTime.UtcNow,
                });
            }

            await appDbContext.SaveChangesAsync();
        }
    }
}
