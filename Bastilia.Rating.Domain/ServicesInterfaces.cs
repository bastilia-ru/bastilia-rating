namespace Bastilia.Rating.Domain
{
    public interface IProjectService
    {
        Task EnsureProjectHasPassword(int projectId);
    }

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
}
