using Bastilia.Rating.Domain.Common;

namespace Bastilia.Rating.Domain
{
    public interface IAchievementService
    {
        Task GrantAchivement(int projectId, int userId, int templateId, int grantedById, string? overrideName);
    }

    public interface IUserDbService
    {
        Task<BastiliaMember> AddUser(int playerId, string nickName, string avatarUrl);
    }

    public interface IKiDbService
    {
        Task AddKogdaIgraGame(int kogdaIgraId, string name, DateOnly begin, DateOnly end, DateTimeOffset lastUpdatedAt);
    }

    public interface IProjectService
    {
        Task<int> CreateProject(string projectName, ProjectType projectType, BrandType brandType, bool OngoingProject, int? JoinrpgProjectId, int? KogdaIgraProjectId, string ProjectUri, IReadOnlyList<UserIdentification> coordinators,
            DateOnly startDate, DateOnly endDate, bool alreadyCompleted, string projectDescription);

        Task CompleteProject(int projectId, DateOnly endDate, ProjectLevel projectLevel, IReadOnlyList<AchievementTemplateInput> achievementTemplates);

        Task AddAchievementTemplates(int projectId, IReadOnlyList<AchievementTemplateInput> achievementTemplates);

        Task SetProjectLevel(int projectId, ProjectLevel projectLevel);

        Task UpdateCoordinators(int projectId, IReadOnlyList<UserIdentification> coordinators);

        Task UpdateAchievementTemplate(int templateId, string name, string description);
    }

    public interface IProjectNewsService
    {
        Task<ProjectNews> Publish(int projectId, UserIdentification authorUserId, string newsText);
        Task<ProjectNews> RetrySend(int newsId);
    }
}
