using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Database.Entities;

public class UsersBastiliaStatus
{
    public int JoinrpgUserId { get; set; }
    public DateOnly BeginDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public BastiliaStatusType StatusType { get; set; }

    public required User User { get; set; }
}
