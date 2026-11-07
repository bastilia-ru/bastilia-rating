using Telegram.Bot.Exceptions;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal static class TelegramErrorClassifier
{
    public static (bool Retryable, string Message) Classify(Exception exception) => exception switch
    {
        ApiRequestException { ErrorCode: 400 or 403 } => (false, exception.Message),
        ApiRequestException => (true, exception.Message),
        RequestException => (true, exception.Message),
        HttpRequestException => (true, exception.Message),
        TimeoutException => (true, exception.Message),
        _ => (true, exception.Message),
    };
}
