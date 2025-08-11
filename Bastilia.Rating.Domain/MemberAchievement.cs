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
    IBastiliaProjectLink? ProjectLink)
{
    public bool NotExpired { get; } = RemoveDate is null && (ExpirationDate is null || ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Now);
}
