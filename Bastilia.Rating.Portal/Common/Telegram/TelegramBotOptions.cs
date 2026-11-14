namespace Bastilia.Rating.Portal.Common.Telegram;

public class TelegramBotOptions
{
    public long BotId { get; set; }
    public string BotSecret { get; set; } = "";
    public long ChatId { get; set; }
    public TelegramProxyOptions? Proxy { get; set; }
    public string WebhookSecret { get; set; } = "";
    public TelegramTransport Transport { get; set; } = TelegramTransport.Webhook;
    public bool Enabled => BotId > 0 && !string.IsNullOrEmpty(BotSecret);
}

/// <summary>
/// Как приложение получает апдейты Telegram. Webhook требует входящей связности
/// (Telegram → наш хостинг); long polling работает через исходящее соединение
/// и не зависит от того, достучивается ли Telegram до сервера.
/// </summary>
public enum TelegramTransport
{
    Webhook,
    LongPolling,
}
