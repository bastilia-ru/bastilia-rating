using Microsoft.Extensions.Options;
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

    private static async Task<IResult> HandleWebhookAsync(
        Update update,
        TelegramUpdateHandler updateHandler,
        IOptions<TelegramBotOptions> options,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var providedSecret = httpContext.Request.Headers[SecretTokenHeader].FirstOrDefault();
        if (!TelegramWebhookSecretVerifier.IsValid(options.Value.WebhookSecret, providedSecret))
        {
            return TypedResults.Unauthorized();
        }

        await updateHandler.HandleStartCommandAsync(update, cancellationToken);
        return TypedResults.Ok();
    }
}
