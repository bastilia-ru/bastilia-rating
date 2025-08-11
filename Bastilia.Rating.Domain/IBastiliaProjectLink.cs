namespace Bastilia.Rating.Domain
{
    public interface IBastiliaProjectLink
    {
        int BastiliaProjectId { get; }
        string ProjectName { get; }
    }
}