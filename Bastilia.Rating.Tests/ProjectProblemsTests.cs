namespace Bastilia.Rating.Tests;

public class ProjectProblemsTests
{
    private static Domain.BastiliaProject CreateProject(
        DateOnly? endDate = null,
        DateOnly? plannedEndDate = null,
        ProjectLevel? projectLevel = null,
        int templatesCount = 0,
        int achievementsCount = 0,
        bool ongoing = false,
        NewsCadence newsCadence = NewsCadence.Monthly,
        DateTimeOffset? lastNewsDate = null)
        => new(1, "Test", ProjectType.MasteringGame, BrandType.Bastilia, ongoing, null, null, null, null, [], endDate, projectLevel, "", new Uri("https://example.com"), null, null, null, plannedEndDate, templatesCount, achievementsCount, lastNewsDate, newsCadence);

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

    [Fact]
    public void OngoingProject_NewsOlderThanCadence_HasNewsOverdueProblem()
    {
        var project = CreateProject(ongoing: true, lastNewsDate: DateTime.UtcNow.AddDays(-40));

        project.Problems.HasFlag(ProjectProblem.NewsOverdue).ShouldBeTrue();
        project.HasProblems.ShouldBeTrue();
    }

    [Fact]
    public void OngoingProject_FreshNews_HasNoNewsOverdueProblem()
    {
        var project = CreateProject(ongoing: true, lastNewsDate: DateTime.UtcNow.AddDays(-10));

        project.Problems.HasFlag(ProjectProblem.NewsOverdue).ShouldBeFalse();
    }

    [Fact]
    public void OngoingProject_NeverCadence_HasNoNewsOverdueProblem()
    {
        var project = CreateProject(ongoing: true, newsCadence: NewsCadence.Never, lastNewsDate: DateTime.UtcNow.AddDays(-1000));

        project.Problems.HasFlag(ProjectProblem.NewsOverdue).ShouldBeFalse();
    }

    [Fact]
    public void OngoingProject_WithoutAnyNews_HasNoNewsOverdueProblem()
    {
        var project = CreateProject(ongoing: true, lastNewsDate: null);

        project.Problems.HasFlag(ProjectProblem.NewsOverdue).ShouldBeFalse();
    }

    [Fact]
    public void FutureProject_OldNews_HasNoNewsOverdueProblem()
    {
        var project = CreateProject(ongoing: false, lastNewsDate: DateTime.UtcNow.AddDays(-100));

        project.Problems.HasFlag(ProjectProblem.NewsOverdue).ShouldBeFalse();
    }

    [Fact]
    public void CompletedProject_OldNews_HasNoNewsOverdueProblem()
    {
        var project = CreateProject(ongoing: true, endDate: new DateOnly(2024, 1, 1), lastNewsDate: DateTime.UtcNow.AddDays(-100));

        project.Problems.HasFlag(ProjectProblem.NewsOverdue).ShouldBeFalse();
    }

    [Fact]
    public void OngoingProject_NewsExactlyOnBoundary_HasNoNewsOverdueProblem()
    {
        var project = CreateProject(ongoing: true, newsCadence: NewsCadence.Monthly, lastNewsDate: DateTime.UtcNow.AddDays(-30));

        project.Problems.HasFlag(ProjectProblem.NewsOverdue).ShouldBeFalse();
    }
}
