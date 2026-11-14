using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Получает апдейты Telegram через long polling (getUpdates): приложение само
/// держит исходящий long-lived запрос. Работает там, куда Telegram не может
/// достучаться webhook'ом, — нужен только исходящий доступ (в т.ч. через прокси).
/// Апдейты, пришедшие до старта сервиса, игнорируются; offset не персистится,
/// поэтому после рестарта возможен повтор последнего обработанного апдейта.
/// Внимание: Bot API допускает ровно одного poller'а на бота — не поднимать
/// больше одной реплики (см. leader election, joinrpg-net#6220).
/// </summary>
internal class TelegramLongPollingService(
    ILogger<TelegramLongPollingService> logger,
    IOptions<TelegramBotOptions> options,
    ITelegramBotApi botApi,
    TelegramUpdateHandler updateHandler) : BackgroundService
{
    private const int PollingTimeoutSeconds = 25;

    internal TimeSpan ErrorRetryDelay { get; init; } = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Telegram не настроен, long polling не запускается");
            return;
        }

        if (options.Value.Transport is not TelegramTransport.LongPolling)
        {
            return;
        }

        try
        {
            // Снимаем webhook, иначе getUpdates апдейтов не получит.
            // drop_pendingUpdates не выставляется: накопленные апдейты дойдут через polling.
            await botApi.DeleteWebhookAsync(stoppingToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Не удалось снять webhook Telegram перед запуском long polling");
        }

        var offset = await SkipAlreadyArrivedUpdatesAsync(stoppingToken);
        logger.LogInformation("Long polling Telegram запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            IReadOnlyList<Update> updates;
            try
            {
                updates = await botApi.GetUpdatesAsync(offset, PollingTimeoutSeconds, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Остановка сервиса или отмена со стороны Telegram-клиента.
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Ошибка long polling Telegram, повтор через {RetryDelay}", ErrorRetryDelay);
                try
                {
                    await Task.Delay(ErrorRetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                continue;
            }

            foreach (var update in updates)
            {
                // Offset сдвигаем до обработки, чтобы «ядовитый» апдейт
                // с ошибкой не зациклил доставку.
                offset = update.Id + 1;
                try
                {
                    await updateHandler.HandleStartCommandAsync(update, stoppingToken);
                    TelegramMetrics.WebhookProcessed();
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Не удалось обработать апдейт Telegram {UpdateId}", update.Id);
                }
            }
        }
    }

    private async Task<int> SkipAlreadyArrivedUpdatesAsync(CancellationToken cancellationToken)
    {
        // offset=-1 возвращает только последний апдейт; подтверждаем его
        // (и всё до него), не обрабатывая — старое не интересует.
        var latest = await botApi.GetUpdatesAsync(offset: -1, timeoutSeconds: 0, cancellationToken);
        return latest.Count > 0 ? latest[^1].Id + 1 : 0;
    }
}
