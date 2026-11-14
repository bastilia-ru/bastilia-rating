using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal class TelegramBotApi(IOptions<TelegramBotOptions> options) : ITelegramBotApi
{
    private readonly Lazy<TelegramBotClient> client = new(() =>
        new TelegramBotClient($"{options.Value.BotId}:{options.Value.BotSecret}",
            TelegramHttpClientFactory.Create(options.Value.Proxy)));

    public async Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
        => await client.Value.SendMessage(
            chatId.Value,
            html.Contents,
            ParseMode.Html,
            linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            cancellationToken: cancellationToken);

    public async Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default)
    {
        var me = await client.Value.GetMe(cancellationToken);
        return me.Username;
    }

    public async Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
        => await client.Value.SetWebhook(
            url,
            secretToken: secret,
            allowedUpdates: [UpdateType.Message],
            cancellationToken: cancellationToken);

    public async Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
        => await client.Value.DeleteWebhook(dropPendingUpdates: true, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Update>> GetUpdatesAsync(int offset, TimeSpan timeout, CancellationToken cancellationToken = default)
        // Bot API принимает таймаут в целых секундах, округляем вверх.
        => await client.Value.GetUpdates(offset, timeout: (int)Math.Ceiling(timeout.TotalSeconds), allowedUpdates: [UpdateType.Message], cancellationToken: cancellationToken);
}
