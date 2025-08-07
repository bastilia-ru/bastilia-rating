namespace Bastilia.Rating.Database.Entities;

public class AchievementTemplate
{
    public int AchievementTemplateId { get; set; }
    public int? ProjectId { get; set; }
    public int? OwnerId { get; set; }
    public string AchievementName { get; set; }
    public string AchievementDescription { get; set; }
    public string AchievementImageUrl { get; set; }
    public int AchievementRatingValue { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public BastiliaProject Project { get; set; }
    public User Owner { get; set; }
    public ICollection<Achievement> Achievements { get; set; }
}
