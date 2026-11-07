namespace Bastilia.Rating.Tests;

public class ProjectLevelSchemeTests
{
    [Fact]
    public void NullLevel_GetAchievementRatingValues_Throws()
    {
        ProjectLevel? level = null;

        var exception = Should.Throw<InvalidOperationException>(() => level.GetAchievementRatingValues());

        exception.Message.ShouldContain("уровень");
    }

    [Theory]
    [InlineData(ProjectLevel.XS, 1)]
    [InlineData(ProjectLevel.S, 2)]
    [InlineData(ProjectLevel.M, 3)]
    [InlineData(ProjectLevel.L, 3)]
    public void Level_GetAchievementRatingValues_ReturnsScheme(ProjectLevel level, int expectedCount)
    {
        level.GetAchievementRatingValues().Count.ShouldBe(expectedCount);
    }

    [Theory]
    [InlineData(ProjectLevel.XS, 1)]
    [InlineData(ProjectLevel.S, 2)]
    [InlineData(ProjectLevel.M, 3)]
    [InlineData(ProjectLevel.L, 3)]
    public void ValidTemplateCount_ValidateTemplateCount_Passes(ProjectLevel level, int templateCount)
    {
        Should.NotThrow(() => level.ValidateTemplateCount(templateCount));
    }

    [Theory]
    [InlineData(ProjectLevel.XS, 0)]
    [InlineData(ProjectLevel.XS, 2)]
    [InlineData(ProjectLevel.S, 1)]
    [InlineData(ProjectLevel.M, 2)]
    [InlineData(ProjectLevel.L, 4)]
    public void WrongTemplateCount_ValidateTemplateCount_Throws(ProjectLevel level, int templateCount)
    {
        Should.Throw<ArgumentException>(() => level.ValidateTemplateCount(templateCount));
    }
}
