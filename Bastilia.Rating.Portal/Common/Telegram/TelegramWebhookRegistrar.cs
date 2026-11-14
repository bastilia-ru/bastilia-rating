using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Регистрирует webhook Telegram при старте приложения. Идемпотентно:
/// setWebhook с теми же параметрами просто перезаписывает то же значение.
/// Telegram — опциональная интеграция, ошибки не должны ронять приложение.
/// </summary>
internal class TelegramWebhookRegistrar(
    ILogger<TelegramWebhookRegistrar> logger,
    IOptions<TelegramBotOptions> options,
    IOptions<JoinRpgHostNamesOptions> hostNames,
    ITelegramBotApi botApi) : BackgroundService
{
    public const string WebhookPath = "/api/telegram/webhook";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Telegram не настроен, webhook не регистрируется");
            return;
        }

        if (options.Value.Transport is not TelegramTransport.Webhook)
        {
            return;
        }

        if (string.IsNullOrEmpty(options.Value.WebhookSecret))
        {
            logger.LogInformation("Telegram:WebhookSecret не задан, webhook не регистрируется");
            return;
        }

        var webhookUrl = $"https://{hostNames.Value.RatingHost}{WebhookPath}";

        try
        {
            // Сначала снимаем старый webhook (и выкидываем накопившиеся апдейты),
            // чтобы setWebhook не получил конфликт «webhook already set».
            await botApi.DeleteWebhookAsync(stoppingToken);
            await botApi.SetWebhookAsync(webhookUrl, options.Value.WebhookSecret, stoppingToken);
            logger.LogInformation("Webhook Telegram зарегистрирован: {WebhookUrl}", webhookUrl);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Не удалось зарегистрировать webhook Telegram");
        }
    }
}
