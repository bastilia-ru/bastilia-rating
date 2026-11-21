using VerifyXunit;

namespace Bastilia.Rating.Tests;

public class ProjectProblemDescriptionsSnapshotTests : VerifyBase
{
    public ProjectProblemDescriptionsSnapshotTests() : base()
    {
    }

    [Fact]
    public Task DescribeProblems_AllFlags_ProducesStableTexts()
        => Verify((ProjectProblem.PlannedEndDatePassed | ProjectProblem.CompletedWithoutTemplates
            | ProjectProblem.CompletedWithoutLevel | ProjectProblem.TemplatesWithoutAchievements
            | ProjectProblem.NewsOverdue).DescribeProblems());
}
