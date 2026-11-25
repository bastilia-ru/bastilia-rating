namespace Bastilia.Rating.Tests;

public class AchievementTemplateInputHelperTests
{
    [Theory]
    [InlineData(ProjectLevel.XS, new[] { 1 })]
    [InlineData(ProjectLevel.S, new[] { 3, 1 })]
    [InlineData(ProjectLevel.M, new[] { 5, 3, 1 })]
    [InlineData(ProjectLevel.L, new[] { 7, 4, 1 })]
    public void CreateDefaults_ReturnsSchemeRatingValues(ProjectLevel level, int[] expected)
    {
        var templates = AchievementTemplateInputHelper.CreateDefaults(level);

        templates.Length.ShouldBe(expected.Length);
        templates.Select(t => t.RatingValue).ShouldBe(expected);
        templates.All(t => !string.IsNullOrWhiteSpace(t.Description)).ShouldBeTrue();
    }
}
