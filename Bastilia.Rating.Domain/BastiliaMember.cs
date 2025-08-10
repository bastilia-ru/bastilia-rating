
namespace Bastilia.Rating.Domain;

// Domain Models
public record BastiliaMember(
    int JoinrpgUserId,
    string UserName,
    string AvatarUrl,
    string? Slug,
    bool ParticipateInRating,
    IReadOnlyCollection<BastiliaStatusHistory> StatusHistory,
    IReadOnlyCollection<ProjectAdminInfo> HisProjects,
    IReadOnlyCollection<MemberAchievement> Achievements) : IUserLink
{
    public BastiliaFinalStatus CurrentStatus { get; } = CalculateStatus(StatusHistory, Achievements);

    public bool IsActiveMember => CurrentStatus == BastiliaFinalStatus.Active;

    public bool IsPresident { get; } =
        StatusHistory
        .Any(bsh => bsh.StatusType == BastiliaStatusType.President && bsh.IsActive);

    public DateOnly? PresidentUntil { get; } = StatusHistory.SingleOrDefault(bsh => bsh.StatusType == BastiliaStatusType.President && bsh.IsActive)?.EndDate;

    public int? RatingValue { get; } = ParticipateInRating ? CalculateRating(Achievements) : null;

    public IReadOnlyCollection<ProjectAdminInfo> HisActiveProjects { get; } = [.. HisProjects.Where(p => p.IsActive)];

    private static int CalculateRating(IReadOnlyCollection<MemberAchievement> achievements)
    {
        return achievements.Where(a => a.NotExpired).Sum(a => a.RatingValue);
    }

    private static BastiliaFinalStatus CalculateStatus(IReadOnlyCollection<BastiliaStatusHistory> statusHistory, IReadOnlyCollection<MemberAchievement> achievements)
    {
        if (statusHistory.Any(bsh => bsh.StatusType == BastiliaStatusType.Member && bsh.IsActive))
        {
            return BastiliaFinalStatus.Active;
        }
        if (statusHistory.Any(bsh => bsh.StatusType == BastiliaStatusType.Member))
        {
            return BastiliaFinalStatus.Retired;
        }
        if (achievements.Count != 0)
        {
            return BastiliaFinalStatus.Mate;
        }
        return BastiliaFinalStatus.None;
    }
}

public enum BastiliaFinalStatus
{
    None,
    Mate,
    Retired,
    Active,
}