namespace Bastilia.Rating.Domain;

public interface IBastiliaMemberRepository
{
    Task<BastiliaMember?> GetByIdAsync(int userId);
    Task<BastiliaMember?> GetBySlugAsync(string slug);

    Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync();
}

public interface IBastiliaProjectRepository
{
    Task<BastiliaProjectWithDetails?> GetByIdAsync(int projectId);
    Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug);
    Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetActualProjects();
}