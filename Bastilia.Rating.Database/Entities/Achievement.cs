namespace Bastilia.Rating.Database.Entities;

public class Achievement
{
    public int AchievementId { get; set; }
    public int AchievementTemplateId { get; set; }
    public int UserId { get; set; }
    public int GrantedBy { get; set; }
    public DateOnly GrantedDate { get; set; }
    public int? RemovedBy { get; set; }
    public DateOnly? RemovedDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }

    public string? OverrideName { get; set; }

    public required AchievementTemplate Template { get; set; }
    public required User User { get; set; }
    public required User GrantedByUser { get; set; }
    public required User RemovedByUser { get; set; }
}
