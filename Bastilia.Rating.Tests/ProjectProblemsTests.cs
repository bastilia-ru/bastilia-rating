namespace Bastilia.Rating.Tests;

public class ProjectProblemsTests
{
    private static Domain.BastiliaProject CreateProject(
        DateOnly? endDate = null,
        DateOnly? plannedEndDate = null,
        ProjectLevel? projectLevel = null,
        int templatesCount = 0,
        int achievementsCount = 0)
        => new(1, "Test", ProjectType.MasteringGame, BrandType.Bastilia, false, null, null, null, null, [], endDate, projectLevel, "", new Uri("https://example.com"), null, null, null, plannedEndDate, templatesCount, achievementsCount);

    [Fact]
    public void CompletedProject_WithoutLevel_HasCompletedWithoutLevelProblem()
    {
        var project = CreateProject(endDate: new DateOnly(2024, 1, 1), templatesCount: 1, achievementsCount: 1);

        project.Problems.HasFlag(ProjectProblem.CompletedWithoutLevel).ShouldBeTrue();
        project.HasProblems.ShouldBeTrue();
    }

    [Fact]
    public void CompletedProject_WithLevel_HasNoLevelProblem()
    {
        var project = CreateProject(endDate: new DateOnly(2024, 1, 1), projectLevel: ProjectLevel.M, templatesCount: 1, achievementsCount: 1);

        project.Problems.HasFlag(ProjectProblem.CompletedWithoutLevel).ShouldBeFalse();
    }

    [Fact]
    public void FutureProject_WithoutLevel_HasNoLevelProblem()
    {
        var project = CreateProject(plannedEndDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30));

        project.HasProblems.ShouldBeFalse();
    }
}
