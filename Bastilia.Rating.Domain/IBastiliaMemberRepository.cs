namespace Bastilia.Rating.Domain;

public interface IBastiliaMemberRepository
{
    Task<BastiliaMember?> GetByIdAsync(int userId);
    Task<BastiliaMember?> GetBySlugAsync(string slug);

    Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync();
}

public interface IBastiliaProjectRepository
{
    Task<BastiliaProject?> GetByIdAsync(int projectId);
    Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects();
    Task<IReadOnlyCollection<BastiliaProject>> GetActualProjects();
}