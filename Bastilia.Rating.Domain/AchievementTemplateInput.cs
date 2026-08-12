namespace Bastilia.Rating.Domain;

public sealed record AchievementTemplateInput(string Name, string Description)
{
    public string Name { get; set; } = Name;
    public string Description { get; set; } = Description;
}

public static class AchievementTemplateInputHelper
{
    public static AchievementTemplateInput[] CreateDefaults(ProjectLevel level)
    {
        return [.. level.GetDefaultAchievementDescriptions().Select(desc => new AchievementTemplateInput("", desc))];
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
