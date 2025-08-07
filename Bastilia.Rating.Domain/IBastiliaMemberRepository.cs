namespace Bastilia.Rating.Domain;

public interface IBastiliaMemberRepository
{
    Task<BastiliaMember?> GetByIdAsync(int userId);
}