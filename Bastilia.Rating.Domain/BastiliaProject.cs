namespace Bastilia.Rating.Domain;

public record BastiliaProject(int BastiliaProjectId,
                              string ProjectName,
                              ProjectType ProjectType,
                              BrandType BrandType,
                              bool OngoingProject,
                              bool? ProjectOfTheYear,
                              int? JoinrpgProjectId,
                              int? KogdaIgraProjectId,
                              string? ProjectUri,
                              IReadOnlyCollection<IUserLink> Coordinators,
                              DateOnly? EndDate,
                              string HowToHelp,
                              Uri ProjectIconUri,
                              string? Slug,
                              string? Password,
                              DateTimeOffset? DeletedAt,
                              DateTimeOffset? LastUpdatedAt
                              ) : IBastiliaProjectLink
{
    public ProjectStatus Status { get; } = CalculateStatus(OngoingProject, EndDate, DeletedAt);
    public bool HelpRequired { get; } = !string.IsNullOrWhiteSpace(HowToHelp);

    private static ProjectStatus CalculateStatus(bool ongoingProject, DateOnly? endDate, DateTimeOffset? deletedAt)
    {
        return (ongoingProject, endDate, deletedAt)
            switch
        {
            (_, _, DateTimeOffset) => ProjectStatus.Deleted,
            (_, DateOnly, _) => ProjectStatus.Completed,
            (true, _, _) => ProjectStatus.Ongoing,
            (false, _, _) => ProjectStatus.Future,
        };
    }
}

public enum ProjectStatus
{
    Completed,
    Future,
    Ongoing,
    Deleted
}