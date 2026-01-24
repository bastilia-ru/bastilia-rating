namespace Bastilia.Rating.Portal.Client.Components.Calendar;

internal class DateRangeHelper
{
    public static string GetRangeString(DateOnly start, DateOnly end)
    {
        if (start.Month == end.Month && start.Day == end.Day)
        {
            return start.ToString("d MMMM");
        }
        else if (start.Month == end.Month)
        {
            return $"{start.Day}—{end:d MMMM}";
        }
        else
        {
            return $"{start.ToShortDateString()}—{end.ToShortDateString()}";
        }
    }
}
