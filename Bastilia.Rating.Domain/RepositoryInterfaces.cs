using JoinRpg.Common.PrimitiveTypes;

namespace Bastilia.Rating.Domain;

public interface IBastiliaMemberRepository
{
    Task<BastiliaMember?> GetByIdAsync(int userId);
    Task<BastiliaMember?> GetBySlugAsync(string slug);
    Task<BastiliaMember?> GetByTelegramChatIdAsync(TelegramChatId chatId);

    Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync();

    Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync();

    Task<IReadOnlyCollection<ActiveUserTelegramInfo>> GetActiveUsersWithTelegramAsync();

    Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory();
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year);
}

public interface IBastiliaProjectRepository
{
    Task<BastiliaProjectWithDetails?> GetByIdAsync(int projectId);
    Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug);
    Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetAllProjects();
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetProjectCalendarFor(int year);
    Task<IReadOnlyCollection<int>> GetProjectIdsForCoordinator(int joinrpgUserId);
}

public interface IBastiliaTemplateRepository
{
    Task<IReadOnlyCollection<AchievementTemplate>> GetAchievementTemplates();
}

public interface IBastiliaKograIgraRepository
{
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetGameCalendarFor(int year);
}

public interface IProjectNewsRepository
{
    Task<ProjectNews?> GetLastForProject(int projectId);
    Task<IReadOnlyCollection<ProjectNews>> GetAllForProject(int projectId);
}
