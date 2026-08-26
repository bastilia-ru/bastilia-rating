using Bastilia.Rating.Database.Entities;

namespace Bastilia.Rating.Database.DbServices
{
    internal class ProjectService(AppDbContext appDbContext) : IProjectService
    {
        public async Task<BastiliaProjectId> CreateProject(string projectName, ProjectType projectType, BrandType brandType, bool OngoingProject,
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
                var user = await appDbContext.Set<Entities.User>().FindAsync(coordinator)
                    ?? throw new InvalidOperationException($"User {coordinator} not found");
                entity.ProjectAdmins.Add(new ProjectAdmin { Project = entity, User = user, AddDate = today });
            }

            await appDbContext.Set<Entities.BastiliaProject>().AddAsync(entity);
            await appDbContext.SaveChangesAsync();

            return entity.BastiliaProjectId;
        }

        public async Task CompleteProject(BastiliaProjectId projectId, DateOnly endDate, ProjectLevel projectLevel, IReadOnlyList<AchievementTemplateInput> achievementTemplates)
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

        public async Task DeleteProject(BastiliaProjectId projectId)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            entity.DeletedAt = DateTimeOffset.UtcNow;

            await appDbContext.SaveChangesAsync();
        }

        public async Task AddAchievementTemplates(BastiliaProjectId projectId, IReadOnlyList<AchievementTemplateInput> achievementTemplates)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .Include(x => x.AchievementTemplates)
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            AddTemplateEntities(entity, achievementTemplates);

            await appDbContext.SaveChangesAsync();
        }

        public async Task SetProjectLevel(BastiliaProjectId projectId, ProjectLevel projectLevel)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            entity.ProjectLevel = projectLevel;

            await appDbContext.SaveChangesAsync();
        }

        public async Task SetNewsCadence(BastiliaProjectId projectId, NewsCadence newsCadence)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            entity.NewsCadence = newsCadence;

            await appDbContext.SaveChangesAsync();
        }

        public async Task UpdateAchievementTemplate(TemplateId templateId, string name, string description)
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
            var ratingValues = achievementTemplates.Select(t => t.RatingValue).ToList();
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
                    AchievementRatingValue = achievementTemplates[i].RatingValue,
                    YearlyAchievement = false,
                    CreateDate = DateTime.UtcNow,
                });
            }
        }

        public async Task UpdateCoordinators(BastiliaProjectId projectId, IReadOnlyList<UserIdentification> coordinators)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>()
                .Include(x => x.ProjectAdmins)
                .FirstOrDefaultAsync(x => x.BastiliaProjectId == projectId) ?? throw new InvalidOperationException();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var newCoordinatorIds = coordinators.ToHashSet();

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
