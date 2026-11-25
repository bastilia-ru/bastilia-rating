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

    public static int GetAchievementRatingValue(this ProjectLevel level, int templateIndex)
    {
        var ratingValues = level.GetAchievementRatingValues();
        return ratingValues[Math.Min(templateIndex, ratingValues.Count - 1)];
    }

    public static void ValidateRatingValues(this ProjectLevel level, IReadOnlyList<int> ratingValues)
    {
        if (ratingValues.Count == 0)
        {
            throw new ArgumentException("Нужно добавить хотя бы один шаблон ачивки", nameof(ratingValues));
        }
        var scheme = level.GetAchievementRatingValues();
        var invalidValue = ratingValues.FirstOrDefault(v => !scheme.Contains(v));
        if (ratingValues.Any(v => !scheme.Contains(v)))
        {
            throw new ArgumentException(
                $"Балл шаблона {invalidValue} не входит в схему уровня {level} (допустимые баллы: {string.Join(", ", scheme)})",
                nameof(ratingValues));
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
