namespace Bastilia.Rating.Domain;

public enum NewsCadence
{
    Weekly,
    Biweekly,
    Monthly,
    Quarterly,
    Yearly,
    Never
}

public static class NewsCadenceExtensions
{
    public static string GetNewsCadenceDescription(this NewsCadence cadence) =>
        cadence switch
        {
            NewsCadence.Weekly => "Еженедельно",
            NewsCadence.Biweekly => "Раз в 2 недели",
            NewsCadence.Monthly => "Раз в месяц",
            NewsCadence.Quarterly => "Раз в квартал",
            NewsCadence.Yearly => "Раз в год",
            NewsCadence.Never => "Никогда",
            _ => throw new ArgumentOutOfRangeException(nameof(cadence), cadence, null)
        };

    public static int GetIntervalDays(this NewsCadence cadence) =>
        cadence switch
        {
            NewsCadence.Weekly => 7,
            NewsCadence.Biweekly => 14,
            NewsCadence.Monthly => 30,
            NewsCadence.Quarterly => 92,
            NewsCadence.Yearly => 365,
            _ => throw new ArgumentOutOfRangeException(nameof(cadence), cadence, null)
        };
}
