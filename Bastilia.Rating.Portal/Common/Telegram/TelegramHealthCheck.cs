using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Чек доступности Telegram API. Опциональная интеграция: без тега "ready",
/// чтобы выключенный Telegram не задерживал /health/ready.
/// </summary>
internal class TelegramHealthCheck(
    IOptions<TelegramBotOptions> options,
    IServiceProvider serviceProvider,
    ILogger<TelegramHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var telegramOptions = options.Value;
        if (!telegramOptions.Enabled)
        {
            return HealthCheckResult.Degraded("Telegram выключен");
        }

        try
        {
            // Клиент достаём здесь, а не через конструктор: TelegramBotClient падает уже при
            // создании, если токен кривой, и это тоже должно стать диагнозом чека.
            var api = serviceProvider.GetRequiredService<ITelegramBotApi>();
            var username = await api.GetMyUserNameAsync(cancellationToken);
            return HealthCheckResult.Healthy("Подключен: " + username);
        }
        catch (OperationCanceledException)
        {
            // Отмена — это сработавший таймаут чека или ушедший клиент. Пусть движок
            // health-чеков отчитается про таймаут, а не мы про «сломанный Telegram».
            throw;
        }
        catch (ApiRequestException exception)
        {
            logger.LogError(exception, "Telegram API вернул ошибку при проверке здоровья. Error code={telegramErrorCode}", exception.ErrorCode);
            return HealthCheckResult.Unhealthy($"Telegram API вернул ошибку {exception.ErrorCode}: {exception.Message}", exception);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ошибка при обращении к Telegram API при проверке здоровья");
            return HealthCheckResult.Unhealthy("Ошибка при обращении к Telegram API: " + exception.Message, exception);
        }
    }
}
