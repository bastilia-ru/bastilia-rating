namespace Bastilia.Rating.Domain;

public static class ProjectLevelExtensions
{
    public static IReadOnlyList<int> GetAchievementRatingValues(this ProjectLevel level) => level switch
    {
        ProjectLevel.XS => [1],
        ProjectLevel.S => [3, 1],
        ProjectLevel.M => [5, 3, 1],
        ProjectLevel.L => [7, 4, 1],
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };
}
