namespace Bastilia.Rating.Database.Entities;

public class AchievementTemplate
{
    public int AchievementTemplateId { get; set; }
    public int? ProjectId { get; set; }
    public int? OwnerId { get; set; }
    public required string AchievementName { get; set; }
    public required string AchievementDescription { get; set; }
    public required string AchievementImageUrl { get; set; }
    public int AchievementRatingValue { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public required BastiliaProject Project { get; set; }
    public required User Owner { get; set; }
    public ICollection<Achievement> Achievements { get; set; } = [];
}
