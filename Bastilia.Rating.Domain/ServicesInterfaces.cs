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
}
