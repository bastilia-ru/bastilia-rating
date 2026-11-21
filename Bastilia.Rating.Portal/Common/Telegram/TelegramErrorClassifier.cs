using Telegram.Bot.Exceptions;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal static class TelegramErrorClassifier
{
    /// <summary>
    /// Классифицирует ошибку отправки в Telegram.
    /// </summary>
    /// <returns>
    /// <c>Retryable</c> — стоит ли повторять отправку; <c>Category</c> — стабильная
    /// низкокардинальная категория (тег reason метрики, см. <see cref="TelegramMetrics" />);
    /// <c>Message</c> — исходный текст ошибки для логов и показа пользователю. Текст исключения
    /// в теги метрики класть нельзя: Prometheus-экспортер хранит каждое значение тега в памяти
    /// навсегда, и текст с переменными частями раздует серии.
    /// </returns>
    public static (bool Retryable, string Category, string Message) Classify(Exception exception) => exception switch
    {
        ApiRequestException { ErrorCode: 400 } => (false, "bad_request", exception.Message),
        ApiRequestException { ErrorCode: 403 } => (false, "forbidden", exception.Message),
        ApiRequestException => (true, "api_error", exception.Message),
        RequestException => (true, "request_failed", exception.Message),
        HttpRequestException => (true, "http_error", exception.Message),
        TimeoutException => (true, "timeout", exception.Message),
        _ => (true, "unknown", exception.Message),
    };
}
