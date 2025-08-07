namespace Bastilia.Rating.Database.Entities;

public class User
{
    public int JoinrpgUserId { get; set; }
    public required string Username { get; set; } 
    public required string AvatarUrl { get; set; }
    public bool ParticipateInRating { get; set; }

    public ICollection<ProjectAdmin> ProjectAdmins { get; set; } = [];
    public ICollection<AchievementTemplate> OwnedAchievementTemplates { get; set; } = [];
    public ICollection<Achievement> Achievements { get; set; } = [];
    public ICollection<UsersBastiliaStatus> BastiliaStatuses { get; set; } = [];
}
