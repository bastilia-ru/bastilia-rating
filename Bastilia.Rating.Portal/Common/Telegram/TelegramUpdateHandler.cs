using Bastilia.Rating.Domain;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal class TelegramUpdateHandler(
    IBastiliaMemberRepository memberRepository,
    ITelegramBotApi botApi,
    WelcomeMessageBuilder welcomeMessageBuilder,
    IOptions<JoinRpgHostNamesOptions> hostNames,
    ILogger<TelegramUpdateHandler> logger)
{
    public async Task HandleStartCommandAsync(Update update, CancellationToken cancellationToken = default)
    {
        var text = update.Message?.Text;
        if (text is null || (text != "/start" && !text.StartsWith("/start@") && !text.StartsWith("/start ")))
        {
            logger.LogDebug("Апдейт Telegram не содержит команду /start, игнорируем");
            return;
        }

        var chatId = update.Message!.Chat.Id;
        var member = await memberRepository.GetByTelegramChatIdAsync(new TelegramChatId(chatId));
        var message = welcomeMessageBuilder.BuildMessage(member, hostNames.Value.RatingHost);

        try
        {
            await botApi.SendHtmlMessageAsync(new TelegramChatId(chatId), new TelegramHtmlString(message), cancellationToken);
        }
        catch (Exception exception)
        {
            var (retryable, errorMessage) = TelegramErrorClassifier.Classify(exception);
            logger.LogWarning(exception, "Не удалось отправить приветствие на /start в Telegram (повторяемая ошибка: {Retryable}): {ErrorMessage}", retryable, errorMessage);
        }
    }
}
