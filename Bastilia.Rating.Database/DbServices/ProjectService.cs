using Bastilia.Rating.Database.Entities;
using JoinRpg.Common.PrimitiveTypes;

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

        public async Task CompleteProject(int projectId, DateOnly endDate, ProjectLevel projectLevel, IReadOnlyList<AchievementTemplateInput> achievementTemplates)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .Include(x => x.AchievementTemplates)
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            entity.EndDate = endDate;
            entity.OngoingProject = false;
            entity.ProjectLevel = projectLevel;

            AddTemplateEntities(entity, achievementTemplates);

            await appDbContext.SaveChangesAsync();
        }

        public async Task AddAchievementTemplates(int projectId, IReadOnlyList<AchievementTemplateInput> achievementTemplates)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .Include(x => x.AchievementTemplates)
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            AddTemplateEntities(entity, achievementTemplates);

            await appDbContext.SaveChangesAsync();
        }

        public async Task SetProjectLevel(int projectId, ProjectLevel projectLevel)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            entity.ProjectLevel = projectLevel;

            await appDbContext.SaveChangesAsync();
        }

        public async Task UpdateAchievementTemplate(int templateId, string name, string description)
        {
            var entity = await appDbContext.Set<Entities.AchievementTemplate>()
                .FirstOrDefaultAsync(x => x.AchievementTemplateId == templateId) ?? throw new InvalidOperationException();

            entity.AchievementName = name;
            entity.AchievementDescription = description;

            await appDbContext.SaveChangesAsync();
        }

        private void AddTemplateEntities(Entities.BastiliaProject entity, IReadOnlyList<AchievementTemplateInput> achievementTemplates)
        {
            var projectLevel = entity.ProjectLevel ?? throw new InvalidOperationException("У проекта не проставлен уровень");
            var startIndex = entity.AchievementTemplates.Count(t => t.DeletedDate == null);
            var ratingValues = Enumerable.Range(startIndex, achievementTemplates.Count)
                .Select(i => projectLevel.GetAchievementRatingValue(i))
                .ToList();
            projectLevel.ValidateRatingValues(ratingValues);

            for (var i = 0; i < achievementTemplates.Count; i++)
            {
                appDbContext.Set<Entities.AchievementTemplate>().Add(new Entities.AchievementTemplate
                {
                    Project = entity,
                    Owner = null!,
                    AchievementName = achievementTemplates[i].Name,
                    AchievementDescription = achievementTemplates[i].Description,
                    AchievementImageUrl = entity.ProjectIconUri,
                    AchievementRatingValue = ratingValues[i],
                    YearlyAchievement = false,
                    CreateDate = DateTime.UtcNow,
                });
            }
        }

        public async Task UpdateCoordinators(int projectId, IReadOnlyList<UserIdentification> coordinators)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .Include(x => x.ProjectAdmins)
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var newCoordinatorIds = coordinators.Select(c => c.Value).ToHashSet();

            foreach (var admin in entity.ProjectAdmins.Where(a => a.RemoveDate == null && !newCoordinatorIds.Contains(a.UserId)))
            {
                admin.RemoveDate = today;
            }

            var existingCoordinatorIds = entity.ProjectAdmins.Select(a => a.UserId).ToHashSet();
            foreach (var coordinatorId in newCoordinatorIds.Where(id => !existingCoordinatorIds.Contains(id)))
            {
                var user = await appDbContext.Set<Entities.User>().FindAsync(coordinatorId)
                    ?? throw new InvalidOperationException($"User {coordinatorId} not found");
                entity.ProjectAdmins.Add(new ProjectAdmin { Project = entity, User = user, AddDate = today });
            }

            foreach (var admin in entity.ProjectAdmins.Where(a => a.RemoveDate != null && newCoordinatorIds.Contains(a.UserId)))
            {
                admin.RemoveDate = null;
            }

            await appDbContext.SaveChangesAsync();
        }
    }
}
