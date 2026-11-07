namespace Bastilia.Rating.Domain;

public record NewsSendResult(bool Sent, bool Retryable, string? ErrorMessage)
{
    public static NewsSendResult Success { get; } = new(true, false, null);

    public static NewsSendResult Skipped(string reason) => new(false, false, reason);

    public static NewsSendResult Failure(string error, bool retryable) => new(false, retryable, error);
}

public interface ITelegramNewsSender
{
    Task<NewsSendResult> Send(ProjectNews news, string projectName, string? projectSlug);
}
