namespace Bastilia.Rating.Domain;

public record MemberAchievement(
    string AchievementName,
    string Description,
    Uri AchievementIconUri,
    int RatingValue,
    DateOnly GrantedDate,
    string GrantedBy,
    DateOnly? RemoveDate,
    string RemovedBy,
    DateOnly? ExpirationDate)
{
    public bool NotExpired { get; } = RemoveDate is null && (ExpirationDate is null || ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) > DateTime.Now);
}
