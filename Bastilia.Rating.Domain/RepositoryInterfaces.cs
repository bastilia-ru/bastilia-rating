namespace Bastilia.Rating.Domain;

public interface IBastiliaMemberRepository
{
    Task<BastiliaMember?> GetByIdAsync(int userId);
    Task<BastiliaMember?> GetBySlugAsync(string slug);

    Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync();

    Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync();

    Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory();
}

public interface IBastiliaProjectRepository
{
    Task<BastiliaProjectWithDetails?> GetByIdAsync(int projectId);
    Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug);
    Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetActualProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetProjectsWithoutPasswords();
}

public interface IBastiliaTemplateRepository
{
    Task<IReadOnlyCollection<AchievementTemplate>> GetAchievementTemplates();
}