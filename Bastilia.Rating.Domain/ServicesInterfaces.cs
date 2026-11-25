using JoinRpg.Common.PrimitiveTypes;

namespace Bastilia.Rating.Domain
{
    public interface IAchievementService
    {
        Task GrantAchivement(BastiliaProjectId projectId, UserIdentification userId, TemplateId templateId, UserIdentification grantedById, string? overrideName);
    }

    public interface IUserDbService
    {
        Task<BastiliaMember> AddUser(UserIdentification playerId, string nickName, string avatarUrl, TelegramSocialLink? telegram = null);

        Task<BastiliaMember> UpdateUser(UserIdentification playerId, string? nickName, string? avatarUrl, TelegramSocialLink? telegram = null);
    }

    public interface IKiDbService
    {
        Task AddKogdaIgraGame(int kogdaIgraId, string name, DateOnly begin, DateOnly end, DateTimeOffset lastUpdatedAt);
    }

    public interface IProjectService
    {
        Task<BastiliaProjectId> CreateProject(string projectName, ProjectType projectType, BrandType brandType, bool OngoingProject, int? JoinrpgProjectId, int? KogdaIgraProjectId, string ProjectUri, IReadOnlyList<UserIdentification> coordinators,
            DateOnly startDate, DateOnly endDate, bool alreadyCompleted, string projectDescription);

        Task CompleteProject(BastiliaProjectId projectId, DateOnly endDate, ProjectLevel projectLevel, IReadOnlyList<AchievementTemplateInput> achievementTemplates);

        Task DeleteProject(BastiliaProjectId projectId);

        Task AddAchievementTemplates(BastiliaProjectId projectId, IReadOnlyList<AchievementTemplateInput> achievementTemplates);

        Task SetProjectLevel(BastiliaProjectId projectId, ProjectLevel projectLevel);

        Task SetNewsCadence(BastiliaProjectId projectId, NewsCadence newsCadence);

        Task UpdateCoordinators(BastiliaProjectId projectId, IReadOnlyList<UserIdentification> coordinators);

        Task UpdateAchievementTemplate(TemplateId templateId, string name, string description);
    }

    public interface IProjectNewsService
    {
        Task<ProjectNews> Publish(BastiliaProjectId projectId, UserIdentification authorUserId, string newsText);
        Task<ProjectNews> RetrySend(int newsId);
    }
}
