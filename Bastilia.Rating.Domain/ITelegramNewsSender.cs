namespace Bastilia.Rating.Domain;

public enum NewsSendOutcome
{
    Sent,
    Skipped,
    Failed,
}

public record NewsSendResult(NewsSendOutcome Outcome, bool Retryable, string? ErrorMessage)
{
    public bool Sent => Outcome == NewsSendOutcome.Sent;

    public static NewsSendResult Success { get; } = new(NewsSendOutcome.Sent, false, null);

    public static NewsSendResult Skipped(string reason) => new(NewsSendOutcome.Skipped, false, reason);

    public static NewsSendResult Failure(string error, bool retryable) => new(NewsSendOutcome.Failed, retryable, error);
}

public interface ITelegramNewsSender
{
    Task<NewsSendResult> Send(ProjectNews news, string projectName, string? projectSlug);
}
