using VerifyXunit;

namespace Bastilia.Rating.Tests;

public class ProjectLevelSchemeTests : VerifyBase
{
    public ProjectLevelSchemeTests() : base()
    {
    }

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
    [InlineData(ProjectLevel.XS, 0, 1)]
    [InlineData(ProjectLevel.S, 0, 3)]
    [InlineData(ProjectLevel.S, 1, 1)]
    [InlineData(ProjectLevel.M, 0, 5)]
    [InlineData(ProjectLevel.M, 2, 1)]
    public void GetAchievementRatingValue_ReturnsSchemeValue(ProjectLevel level, int index, int expectedValue)
    {
        level.GetAchievementRatingValue(index).ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(ProjectLevel.S, 2)]
    [InlineData(ProjectLevel.M, 3)]
    [InlineData(ProjectLevel.M, 10)]
    public void GetAchievementRatingValue_ExtraTemplates_GetMinSchemeValue(ProjectLevel level, int index)
    {
        var minValue = level.GetAchievementRatingValues()[^1];

        level.GetAchievementRatingValue(index).ShouldBe(minValue);
    }

    [Fact]
    public void EmptyRatingValues_ValidateRatingValues_Throws()
    {
        Should.Throw<ArgumentException>(() => ProjectLevel.M.ValidateRatingValues([]));
    }

    [Theory]
    [InlineData(ProjectLevel.XS, new[] { 1 })]
    [InlineData(ProjectLevel.M, new[] { 5, 3, 1 })]
    [InlineData(ProjectLevel.M, new[] { 5, 3, 1, 1, 1 })]
    [InlineData(ProjectLevel.M, new[] { 1, 1 })]
    public void SchemeValues_ValidateRatingValues_Passes(ProjectLevel level, int[] ratingValues)
    {
        Should.NotThrow(() => level.ValidateRatingValues(ratingValues));
    }

    [Theory]
    [InlineData(ProjectLevel.XS, new[] { 2 })]
    [InlineData(ProjectLevel.S, new[] { 5, 1 })]
    [InlineData(ProjectLevel.M, new[] { 7, 3, 1 })]
    [InlineData(ProjectLevel.M, new[] { 5, 2 })]
    [InlineData(ProjectLevel.XS, new[] { 3 })]
    public void OutOfScheme_ValidateRatingValues_Throws(ProjectLevel level, int[] ratingValues)
    {
        var exception = Should.Throw<ArgumentException>(() => level.ValidateRatingValues(ratingValues));

        exception.Message.ShouldContain("балл");
    }

    [Fact]
    public Task OutOfScheme_ValidateRatingValues_MessageSnapshot()
    {
        var exception = Should.Throw<ArgumentException>(() => ProjectLevel.M.ValidateRatingValues([4]));
        return Verify(exception.Message);
    }
}
