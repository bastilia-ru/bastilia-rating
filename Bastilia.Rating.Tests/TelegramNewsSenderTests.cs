using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;

namespace Bastilia.Rating.Tests;

public class TelegramNewsSenderTests
{
    private sealed class FakeTelegramBotApi : ITelegramBotApi
    {
        public bool WasCalled { get; private set; }
        public long CapturedChatId { get; private set; }
        public string? CapturedHtml { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task SendHtmlMessageAsync(long chatId, string html, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            CapturedChatId = chatId;
            CapturedHtml = html;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.CompletedTask;
        }
    }

    private static TelegramNewsSender CreateSender(TelegramBotOptions telegramOptions, FakeTelegramBotApi fake) =>
        new(Options.Create(telegramOptions),
            Options.Create(new JoinRpgHostNamesOptions
            {
                MainHost = "joinrpg.ru",
                IdHost = "id.joinrpg.ru",
                KogdaIgraHost = "kogda-igra.ru",
                RatingHost = "rating.bastilia.ru",
            }),
            fake);

    private static ProjectNews CreateNews() =>
        new(1, 2, 3, "Имя Автора", DateTimeOffset.UtcNow, "**текст**", NewsSendStatus.NotSent, null);

    private static TelegramBotOptions EnabledOptions(long chatId = 100) =>
        new() { BotId = 42, BotSecret = "secret", ChatId = chatId };

    [Fact]
    public async Task TelegramDisabled_ReturnsSkipped_WithoutCallingApi()
    {
        var fake = new FakeTelegramBotApi();
        var sender = CreateSender(new TelegramBotOptions { BotId = 0 }, fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Sent.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Telegram не настроен");
        fake.WasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ChatIdZero_ReturnsSkipped()
    {
        var fake = new FakeTelegramBotApi();
        var sender = CreateSender(EnabledOptions(chatId: 0), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Sent.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("ChatId не настроен");
        fake.WasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ApiThrows403_ReturnsFailedNotRetryable()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new ApiRequestException("bot was kicked", 403) };
        var sender = CreateSender(EnabledOptions(), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Sent.ShouldBeFalse();
        result.Retryable.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("bot was kicked");
    }

    [Fact]
    public async Task ApiThrows500_ReturnsFailedRetryable()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new ApiRequestException("internal error", 500) };
        var sender = CreateSender(EnabledOptions(), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Sent.ShouldBeFalse();
        result.Retryable.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("internal error");
    }

    [Fact]
    public async Task ApiSends_ReturnsSuccess()
    {
        var fake = new FakeTelegramBotApi();
        var sender = CreateSender(EnabledOptions(chatId: 100), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Sent.ShouldBeTrue();
        result.Retryable.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
        fake.WasCalled.ShouldBeTrue();
        fake.CapturedChatId.ShouldBe(100);
        fake.CapturedHtml.ShouldContain("<b>");
    }
}
