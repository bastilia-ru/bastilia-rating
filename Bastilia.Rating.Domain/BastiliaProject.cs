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
                              ProjectLevel? ProjectLevel,
                              string HowToHelp,
                              Uri ProjectIconUri,
                              string? Slug,
                              DateTimeOffset? DeletedAt,
                              DateTimeOffset? LastUpdatedAt,
                              DateOnly? PlannedEndDate,
                              int TemplatesCount,
                              int AchievementsCount,
                              DateTimeOffset? LastNewsDate
                              ) : IBastiliaProjectLink
{

    public bool IsActive => Status != ProjectStatus.Completed && Status != ProjectStatus.Deleted;
    public ProjectStatus Status { get; } = CalculateStatus(OngoingProject, EndDate, DeletedAt);
    public bool HelpRequired => !string.IsNullOrWhiteSpace(HowToHelp) && IsActive;
    public ProjectProblem Problems => CalculateProblems(PlannedEndDate, EndDate, Status, TemplatesCount, AchievementsCount, ProjectLevel, DateOnly.FromDateTime(DateTime.UtcNow));
    public bool HasProblems => Problems != ProjectProblem.None;

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

    private static ProjectProblem CalculateProblems(DateOnly? plannedEndDate, DateOnly? endDate, ProjectStatus status, int templatesCount, int achievementsCount, ProjectLevel? projectLevel, DateOnly today)
    {
        var problems = ProjectProblem.None;
        if (endDate is null && plannedEndDate is not null && plannedEndDate < today)
        {
            problems |= ProjectProblem.PlannedEndDatePassed;
        }
        if (status == ProjectStatus.Completed && templatesCount == 0)
        {
            problems |= ProjectProblem.CompletedWithoutTemplates;
        }
        if (status == ProjectStatus.Completed && projectLevel is null)
        {
            problems |= ProjectProblem.CompletedWithoutLevel;
        }
        if (templatesCount > 0 && achievementsCount == 0)
        {
            problems |= ProjectProblem.TemplatesWithoutAchievements;
        }
        return problems;
    }
}

public enum ProjectStatus
{
    Completed,
    Future,
    Ongoing,
    Deleted
}

[Flags]
public enum ProjectProblem
{
    None = 0,
    PlannedEndDatePassed = 1,
    CompletedWithoutTemplates = 2,
    CompletedWithoutLevel = 4,
    TemplatesWithoutAchievements = 8,
}
