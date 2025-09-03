namespace Bastilia.Rating.Domain;

public record MemberAchievement(
    string AchievementName,
    string Description,
    Uri AchievementIconUri,
    int RatingValue,
    DateOnly GrantedDate,
    IUserLink GrantedBy,
    DateOnly? RemoveDate,
    IUserLink? RemovedBy,
    DateOnly? ExpirationDate,
    IBastiliaProjectLink? ProjectLink,
    bool ParticipateInRating,
    IUserLink User,
    Uri UserAvatar)
{
    public bool NotExpired { get; } = RemoveDate is null && (ExpirationDate is null || ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Now);
    public int? RatingForDisplay => ParticipateInRating ? RatingValue : null;
    public bool IsExpired => !NotExpired;
}
