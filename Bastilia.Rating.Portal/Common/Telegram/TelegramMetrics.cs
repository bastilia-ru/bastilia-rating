using System.Diagnostics.Metrics;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Метрики Telegram-интеграции. Meter назван в честь приложения, потому что
/// AddJoinOpenTelemetry экспортирует в Prometheus только meter с именем сервиса
/// и фиксированный список общих meter'ов.
/// </summary>
public static class TelegramMetrics
{
    public const string MeterName = "Bastilia.Rating.Portal";

    private static readonly Meter meter = new(MeterName);

    private static readonly Counter<long> sends = meter.CreateCounter<long>(
        "telegram.sends",
        description: "Отправки сообщений в Telegram");

    private static readonly Counter<long> webhooks = meter.CreateCounter<long>(
        "telegram.webhooks",
        description: "Вызовы Telegram webhook");

    public static void SendSucceeded(string kind)
        => sends.Add(1,
            new KeyValuePair<string, object?>("kind", kind),
            new KeyValuePair<string, object?>("result", "success"));

    public static void SendFailed(string kind, string error, bool retryable)
        => sends.Add(1,
            new KeyValuePair<string, object?>("kind", kind),
            new KeyValuePair<string, object?>("result", "failure"),
            new KeyValuePair<string, object?>("reason", error),
            new KeyValuePair<string, object?>("retryable", retryable.ToString()));

    public static void SendSkipped(string kind, string reason)
        => sends.Add(1,
            new KeyValuePair<string, object?>("kind", kind),
            new KeyValuePair<string, object?>("result", "skipped"),
            new KeyValuePair<string, object?>("reason", reason));

    public static void WebhookProcessed()
        => webhooks.Add(1, new KeyValuePair<string, object?>("result", "processed"));

    public static void WebhookSkipped(string reason)
        => webhooks.Add(1,
            new KeyValuePair<string, object?>("result", "skipped"),
            new KeyValuePair<string, object?>("reason", reason));
}
