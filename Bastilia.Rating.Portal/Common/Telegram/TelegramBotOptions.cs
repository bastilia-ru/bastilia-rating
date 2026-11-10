namespace Bastilia.Rating.Portal.Common.Telegram;

public class TelegramBotOptions
{
    public long BotId { get; set; }
    public string BotSecret { get; set; } = "";
    public long ChatId { get; set; }
    public TelegramProxyOptions? Proxy { get; set; }
    public string WebhookSecret { get; set; } = "";
    public bool Enabled => BotId > 0 && !string.IsNullOrEmpty(BotSecret);
}
