namespace Bastilia.Rating.Portal.Client;

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
            return $"{start.Day}–" + end.ToString("d MMMM");
        }
        return $"{start.ToShortDateString}–{end.ToShortDateString()}";
    }
}