using System.Diagnostics;
using JoinRpg.Common.PrimitiveTypes;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Логирует вызовы Telegram Bot API: метод, длительность, результат.
/// Успешные быстрые вызовы — в Debug (long polling обращается к API постоянно);
/// медленные и падающие — в Information/Warning: именно они и есть симптом
/// проблем со связностью (прокси, пул соединений), а не шум успешной работы.
/// Штатная отмена по токену вызывающего кода — в Debug без метрики:
/// это не поломка. Таймаут HttpClient (TaskCanceledException без запроса
/// отмены) — Warning с метрикой: главный симптом «прокси молчит».
/// </summary>
internal class LoggingTelegramBotApi(ITelegramBotApi inner, ILogger<LoggingTelegramBotApi> logger) : ITelegramBotApi
{
    private static readonly TimeSpan DefaultSlowCallThreshold = TimeSpan.FromSeconds(5);

    // Bot API штатно держит long polling ровно timeout секунд при пустой очереди
    // апдейтов: «медленным» getUpdates считаем только с запасом сверх timeout.
    private static readonly TimeSpan LongPollingSlack = TimeSpan.FromSeconds(10);

    public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
        => LogAsync("SendMessage", $"SendMessage(chat={chatId.Value})", DefaultSlowCallThreshold,
            () => inner.SendHtmlMessageAsync(chatId, html, cancellationToken), cancellationToken);

    public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default)
        => LogAsync("GetMe", "GetMe", DefaultSlowCallThreshold,
            async () => await inner.GetMyUserNameAsync(cancellationToken), cancellationToken);

    public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
        => LogAsync("SetWebhook", $"SetWebhook({url})", DefaultSlowCallThreshold,
            async () => await inner.SetWebhookAsync(url, secret, cancellationToken), cancellationToken);

    public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
        => LogAsync("DeleteWebhook", "DeleteWebhook", DefaultSlowCallThreshold,
            () => inner.DeleteWebhookAsync(cancellationToken), cancellationToken);

    public Task<IReadOnlyList<Update>> GetUpdatesAsync(int offset, TimeSpan timeout, CancellationToken cancellationToken = default)
        => LogAsync("GetUpdates", $"GetUpdates(offset={offset}, timeout={timeout})", timeout + LongPollingSlack,
            () => inner.GetUpdatesAsync(offset, timeout, cancellationToken), cancellationToken);

    private async Task LogAsync(string method, string operation, TimeSpan slowAfter, Func<Task> call, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await call();
            stopwatch.Stop();
            TelegramMetrics.ApiCallFinished(method, stopwatch.Elapsed, result: "success");
            if (stopwatch.Elapsed < slowAfter)
            {
                logger.LogDebug("Telegram API: {Operation} за {Duration} мс", operation, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation("Telegram API: медленный вызов {Operation} — {Duration} мс", operation, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            logger.LogDebug("Telegram API: {Operation} отменён вызывающим кодом за {Duration} мс", operation, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            TelegramMetrics.ApiCallFinished(method, stopwatch.Elapsed, result: exception.GetType().Name);
            logger.LogWarning(exception, "Telegram API: {Operation} не удался за {Duration} мс", operation, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<TResult> LogAsync<TResult>(string method, string operation, TimeSpan slowAfter, Func<Task<TResult>> call, CancellationToken cancellationToken)
    {
        // Statement-lambda, а не expression: иначе лямбда выводится как Func<Task<TResult>>
        // и снова резолвится в generic-перегрузку — бесконечная рекурсия.
        TResult? result = default;
        await LogAsync(method, operation, slowAfter, async () => { result = await call(); }, cancellationToken);
        return result!;
    }
}
