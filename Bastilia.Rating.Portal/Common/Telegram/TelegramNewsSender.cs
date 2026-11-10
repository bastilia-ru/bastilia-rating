using Bastilia.Rating.Domain;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal class TelegramNewsSender(
    IOptions<TelegramBotOptions> options,
    IOptions<JoinRpgHostNamesOptions> hostNames,
    ITelegramBotApi botApi,
    NewsMessageBuilder messageBuilder) : ITelegramNewsSender
{
    public async Task<NewsSendResult> Send(ProjectNews news, string projectName, string? projectSlug)
    {
        if (!options.Value.Enabled)
        {
            TelegramMetrics.SendSkipped("news", "Telegram не настроен");
            return NewsSendResult.Skipped("Telegram не настроен");
        }

        if (options.Value.ChatId == 0)
        {
            TelegramMetrics.SendSkipped("news", "ChatId не настроен");
            return NewsSendResult.Skipped("ChatId не настроен");
        }

        var message = messageBuilder.BuildMessage(news, projectName, projectSlug, hostNames.Value.RatingHost);

        try
        {
            await botApi.SendHtmlMessageAsync(new TelegramChatId(options.Value.ChatId), new TelegramHtmlString(message));
            TelegramMetrics.SendSucceeded("news");
            return NewsSendResult.Success;
        }
        catch (Exception exception)
        {
            var (retryable, errorMessage) = TelegramErrorClassifier.Classify(exception);
            TelegramMetrics.SendFailed("news", errorMessage, retryable);
            return NewsSendResult.Failure(errorMessage, retryable);
        }
    }
}
