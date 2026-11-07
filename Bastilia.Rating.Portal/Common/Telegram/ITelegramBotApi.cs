namespace Bastilia.Rating.Portal.Common.Telegram;

internal interface ITelegramBotApi
{
    Task SendHtmlMessageAsync(long chatId, string html, CancellationToken cancellationToken = default);
}
