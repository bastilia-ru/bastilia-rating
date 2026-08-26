namespace Bastilia.Rating.Domain;

public record ProjectNews(int Id, BastiliaProjectId ProjectId, UserIdentification AuthorUserId, string AuthorName, DateTimeOffset CreatedAt, string Text, NewsSendStatus SendStatus, string? TelegramError);

public enum NewsSendStatus
{
    NotSent,
    Sent,
    Failed
}
