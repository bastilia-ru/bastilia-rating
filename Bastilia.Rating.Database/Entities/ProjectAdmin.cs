namespace Bastilia.Rating.Database.Entities;

public class ProjectAdmin
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public DateOnly AddDate { get; set; }
    public DateOnly? RemoveDate { get; set; }

    public required BastiliaProject Project { get; set; }
    public required User User { get; set; }
}
