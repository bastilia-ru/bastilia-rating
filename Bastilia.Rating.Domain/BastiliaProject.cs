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
                              DateOnly? EndDate) : IBastiliaProjectLink
{
    public ProjectStatus Status { get; set; } = CalculateStatus(OngoingProject, EndDate);

    private static ProjectStatus CalculateStatus(bool ongoingProject, DateOnly? endDate)
    {
        return (ongoingProject, endDate)
            switch
        {
            (_, DateOnly d) => ProjectStatus.Completed,
            (true, _) => ProjectStatus.Ongoing,
            (false, _) => ProjectStatus.Future,
        };
    }
}

public enum ProjectStatus
{
    Completed,
    Future,
    Ongoing,
}