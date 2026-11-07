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

    public static IReadOnlyList<int> GetAchievementRatingValues(this ProjectLevel? level)
        => (level ?? throw new InvalidOperationException("У проекта не проставлен уровень")).GetAchievementRatingValues();

    public static void ValidateTemplateCount(this ProjectLevel level, int templateCount)
    {
        var ratingValues = level.GetAchievementRatingValues();
        if (templateCount != ratingValues.Count)
        {
            throw new ArgumentException(
                $"Для уровня {level} требуется {ratingValues.Count} шаблонов ачивок, передано {templateCount}",
                nameof(templateCount));
        }
    }

    public static string GetSizeDescription(this ProjectLevel level) =>
        level switch
        {
            ProjectLevel.XS => "Какая-то небольшая активность, маленький выезд",
            ProjectLevel.S => "Большая активность, маленькая игра, выезд 15+ человек",
            ProjectLevel.M => "Средняя игра (60–150), конвент 60+ человек, зимняя игра, выезд 30+ человек",
            ProjectLevel.L => "Игра или конвент 150+ человек",
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
}
