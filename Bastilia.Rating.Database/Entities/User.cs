using Microsoft.EntityFrameworkCore;

namespace Bastilia.Rating.Database.Entities;

[PrimaryKey(nameof(JoinRpgUserId))]
public class User
{

    public int JoinRpgUserId { get; set; }
    public required string Username { get; set; }
    public required string AvatarUrl { get; set; }
    public bool ParticipateInRating { get; set; }

    public ICollection<ProjectAdmin> ProjectAdmins { get; set; } = [];
    public ICollection<AchievementTemplate> OwnedAchievementTemplates { get; set; } = [];
    public ICollection<Achievement> Achievements { get; set; } = [];
    public ICollection<UsersBastiliaStatus> BastiliaStatuses { get; set; } = [];
}
