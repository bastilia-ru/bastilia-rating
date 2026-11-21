namespace Bastilia.Rating.Domain;

public static class ProjectProblemExtensions
{
    public static IEnumerable<string> DescribeProblems(this ProjectProblem problems)
    {
        if (problems.HasFlag(ProjectProblem.PlannedEndDatePassed))
        {
            yield return "Завершение проекта не отмечено";
        }
        if (problems.HasFlag(ProjectProblem.CompletedWithoutTemplates))
        {
            yield return "У проекта нет шаблонов ачивок";
        }
        if (problems.HasFlag(ProjectProblem.CompletedWithoutLevel))
        {
            yield return "У проекта не выставлен уровень";
        }
        if (problems.HasFlag(ProjectProblem.TemplatesWithoutAchievements))
        {
            yield return "У проекта нет ачивок";
        }
        if (problems.HasFlag(ProjectProblem.NewsOverdue))
        {
            yield return "Давно нет новостей";
        }
    }
}
