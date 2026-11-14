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
            TelegramMetrics.WebhookSkipped("not_start_command");
            return;
        }

        var chatId = update.Message!.Chat.Id;
        var member = await memberRepository.GetByTelegramChatIdAsync(new TelegramChatId(chatId));
        logger.LogInformation("Команда /start из Telegram: chatId={ChatId}, username=@{TelegramUsername}, найден член клуба: {MemberDescription}",
            chatId,
            update.Message.From?.Username,
            member is null ? "нет (пользователь не привязан)" : $"{member.JoinrpgUserId} ({member.UserName})");
        var message = welcomeMessageBuilder.BuildMessage(member, hostNames.Value.RatingHost);

        try
        {
            await botApi.SendHtmlMessageAsync(new TelegramChatId(chatId), new TelegramHtmlString(message), cancellationToken);
            TelegramMetrics.SendSucceeded("welcome");
        }
        catch (Exception exception)
        {
            var (retryable, errorMessage) = TelegramErrorClassifier.Classify(exception);
            TelegramMetrics.SendFailed("welcome", errorMessage, retryable);
            logger.LogWarning(exception, "Не удалось отправить приветствие на /start в Telegram (повторяемая ошибка: {Retryable}): {ErrorMessage}", retryable, errorMessage);
        }
    }
}
