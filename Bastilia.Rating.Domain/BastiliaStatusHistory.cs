namespace Bastilia.Rating.Domain;

public record BastiliaStatusHistory(
    DateOnly BeginDate,
    BastiliaStatusType StatusType,
    DateOnly? EndDate)
{
    public bool IsActive { get; } = 
        BeginDate.ToDateTime(TimeOnly.MinValue) < DateTime.Now 
        && (EndDate is null || EndDate.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Now);
}
