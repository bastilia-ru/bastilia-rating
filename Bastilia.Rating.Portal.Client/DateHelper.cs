namespace Bastilia.Rating.Portal.Client;

// Русский язык, чтобы быть убежденным, что тут UTF-8
public static class DateHelper
{
    public static string DisplayRange(DateOnly start, DateOnly end)
    {
        if (start.Month == end.Month && start.Day == end.Day)
        {
            return start.ToString("d MMMM");
        }
        if (start.Month == end.Month)
        {
            return $"{start.Day}&#x2013;" + end.ToString("d MMMM");
        }
        return $"{start:d}&#x2013;{end:d}";
    }
}