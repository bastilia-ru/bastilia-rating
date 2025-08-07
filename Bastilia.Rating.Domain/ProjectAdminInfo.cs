namespace Bastilia.Rating.Domain;

public record ProjectAdminInfo(
    int ProjectId,
    string ProjectName,
    DateOnly AddDate,
    DateOnly? RemoveDate)
{
    public bool IsActive { get; } = RemoveDate is null;
}
