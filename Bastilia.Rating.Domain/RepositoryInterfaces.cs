namespace Bastilia.Rating.Domain;

public interface IBastiliaMemberRepository
{
    Task<BastiliaMember?> GetByIdAsync(int userId);
    Task<BastiliaMember?> GetBySlugAsync(string slug);

    Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync();

    Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync();

    Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory();
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year);
}

public interface IBastiliaProjectRepository
{
    Task<BastiliaProjectWithDetails?> GetByIdAsync(int projectId);
    Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug);
    Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetAllProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetProjectsWithoutPasswords();
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetProjectCalendarFor(int year);
}

public interface IBastiliaTemplateRepository
{
    Task<IReadOnlyCollection<AchievementTemplate>> GetAchievementTemplates();
}

public interface IBastiliaKograIgraRepository
{
    Task<IReadOnlyCollection<BastiliaCalendarItem>> GetGameCalendarFor(int year);
}
