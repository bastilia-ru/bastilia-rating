using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal class TelegramBotApi(IOptions<TelegramBotOptions> options) : ITelegramBotApi
{
    private readonly Lazy<TelegramBotClient> client = new(() =>
        new TelegramBotClient($"{options.Value.BotId}:{options.Value.BotSecret}"));

    public async Task SendHtmlMessageAsync(long chatId, string html, CancellationToken cancellationToken = default)
        => await client.Value.SendMessage(
            chatId,
            html,
            ParseMode.Html,
            linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            cancellationToken: cancellationToken);
}
