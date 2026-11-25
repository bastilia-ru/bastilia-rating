namespace Bastilia.Rating.Domain;

public sealed record AchievementTemplateInput(string Name, string Description, int RatingValue)
{
    public string Name { get; set; } = Name;
    public string Description { get; set; } = Description;
    public int RatingValue { get; set; } = RatingValue;
}

public static class AchievementTemplateInputHelper
{
    public static AchievementTemplateInput[] CreateDefaults(ProjectLevel level)
    {
        return [.. level.GetDefaultAchievementDescriptions()
            .Select((desc, i) => new AchievementTemplateInput("", desc, level.GetAchievementRatingValue(i)))];
    }

    private static IReadOnlyList<string> GetDefaultAchievementDescriptions(this ProjectLevel level) => level switch
    {
        ProjectLevel.XS => ["Помог"],
        ProjectLevel.S => ["Главный мастер игры или капитан выезда", "Мастер, волонтер или участник выезда"],
        ProjectLevel.M => ["Главный мастер игры или капитан выезда", "Мастер игры или капитан микрокоманды", "Волонтер или участник выезда"],
        ProjectLevel.L => ["Главный мастер игры", "Мастер игры", "Волонтер"],
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };
}
