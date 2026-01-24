namespace Bastilia.Rating.Domain;

public record class BastiliaCalendarItem(BastiliaCalendarItemType Type, DateOnly StartDate, DateOnly EndDate, string Name, int Id)
{
    public BastiliaCalendarItem(BastiliaCalendarItemType Type, DateOnly StartDate, string Name, int Id) : this(Type, StartDate, StartDate, Name, Id) { }
}

public enum BastiliaCalendarItemType
{
    Unknown = 0,
    Birthday = 1,
    BirthdayParty = 2,
    Project = 3,
}
