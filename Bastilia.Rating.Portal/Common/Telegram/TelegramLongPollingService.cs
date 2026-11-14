using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Получает апдейты Telegram через long polling (getUpdates): приложение само
/// держит исходящий long-lived запрос. Работает там, куда Telegram не может
/// достучаться webhook'ом, — нужен только исходящий доступ (в т.ч. через прокси).
/// Апдейты, пришедшие до старта сервиса, сбрасываются (drop_pending_updates
/// при снятии webhook'а); offset не персистится, поэтому после рестарта
/// возможен повтор последнего обработанного апдейта.
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
            // Снимаем webhook (иначе getUpdates апдейтов не получит).
            // drop_pending_updates: накопленные до старта апдейты нас не интересуют.
            await botApi.DeleteWebhookAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Не удалось снять webhook Telegram перед запуском long polling");
        }

        logger.LogInformation("Long polling Telegram запущен");
        var offset = 0;

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
                    TelegramMetrics.WebhookProcessed("long_polling");
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
}
