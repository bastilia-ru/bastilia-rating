using JoinRpg.Common.PrimitiveTypes;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal interface ITelegramBotApi
{
    Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default);
    Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default);
    Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default);
    Task DeleteWebhookAsync(CancellationToken cancellationToken = default);
}
