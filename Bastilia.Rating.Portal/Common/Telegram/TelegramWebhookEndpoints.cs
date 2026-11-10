using System.Text.Json;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Portal.Common.Telegram;

public static class TelegramWebhookEndpoints
{
    private const string SecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";

    public static WebApplication MapTelegramWebhook(this WebApplication app)
    {
        app.MapPost(TelegramWebhookRegistrar.WebhookPath, HandleWebhookAsync);
        return app;
    }

    // Telegram ретраит всё, что не 2xx, поэтому ошибки обработки логируем
    // и всё равно отвечаем 200.
    private static async Task<IResult> HandleWebhookAsync(
        HttpContext httpContext,
        TelegramUpdateHandler updateHandler,
        IOptions<TelegramBotOptions> options,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(TelegramWebhookEndpoints));
        var providedSecret = httpContext.Request.Headers[SecretTokenHeader].FirstOrDefault();
        if (!TelegramWebhookSecretVerifier.IsValid(options.Value.WebhookSecret, providedSecret))
        {
            return TypedResults.Unauthorized();
        }

        string json;
        using (var reader = new StreamReader(httpContext.Request.Body))
        {
            json = await reader.ReadToEndAsync(cancellationToken);
        }

        Update? update;
        try
        {
            update = JsonSerializer.Deserialize<Update>(json, JsonBotAPI.Options);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Не удалось разобрать апдейт Telegram webhook");
            return TypedResults.Ok();
        }

        if (update is null)
        {
            logger.LogWarning("Пустой апдейт Telegram webhook");
            return TypedResults.Ok();
        }

        await updateHandler.HandleStartCommandAsync(update, cancellationToken);
        return TypedResults.Ok();
    }
}
