namespace Bastilia.Rating.Domain;

public record class BastiliaCalendarItem(BastiliaCalendarItemType Type, DateOnly StartDate, DateOnly EndDate, string Name)
{
    public BastiliaCalendarItem(BastiliaCalendarItemType Type, DateOnly StartDate, string Name) : this(Type, StartDate, StartDate, Name) { }
}

public enum BastiliaCalendarItemType
{
    Unknown = 0,
    Birthday = 1,
    BirthdayParty = 2,
    Project = 3,
}
