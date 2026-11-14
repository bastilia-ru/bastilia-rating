using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Tests;

public class TelegramNewsSenderTests : IDisposable
{
    private readonly MeterListenerCollector collector = new(TelegramMetrics.MeterName);

    private sealed class FakeTelegramBotApi : ITelegramBotApi
    {
        public bool WasCalled { get; private set; }
        public TelegramChatId? CapturedChatId { get; private set; }
        public TelegramHtmlString? CapturedHtml { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
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

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("bastilia_bot");
        public Task<IReadOnlyList<Update>> GetUpdatesAsync(int offset, TimeSpan timeout, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
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
            fake,
            TelegramTestFactory.CreateMessageBuilder());

    private void AssertNewsSendRecorded(string result, string? reason = null, string? retryable = null)
        => collector.Measurements.ShouldContain(m => m.Instrument == "telegram.sends"
            && Equals(m.Tags.GetValueOrDefault("kind"), "news")
            && Equals(m.Tags.GetValueOrDefault("result"), result)
            && (reason == null || Equals(m.Tags.GetValueOrDefault("reason"), reason))
            && (retryable == null || Equals(m.Tags.GetValueOrDefault("retryable"), retryable)));

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

        result.Outcome.ShouldBe(NewsSendOutcome.Skipped);
        result.Sent.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Telegram не настроен");
        fake.WasCalled.ShouldBeFalse();

        AssertNewsSendRecorded("skipped", reason: "Telegram не настроен");
    }

    [Fact]
    public async Task ChatIdZero_ReturnsSkipped()
    {
        var fake = new FakeTelegramBotApi();
        var sender = CreateSender(EnabledOptions(chatId: 0), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Outcome.ShouldBe(NewsSendOutcome.Skipped);
        result.Sent.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("ChatId не настроен");
        fake.WasCalled.ShouldBeFalse();

        AssertNewsSendRecorded("skipped", reason: "ChatId не настроен");
    }

    [Fact]
    public async Task ApiThrows403_ReturnsFailedNotRetryable()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new ApiRequestException("bot was kicked", 403) };
        var sender = CreateSender(EnabledOptions(), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Outcome.ShouldBe(NewsSendOutcome.Failed);
        result.Sent.ShouldBeFalse();
        result.Retryable.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("bot was kicked");

        AssertNewsSendRecorded("failure", reason: "bot was kicked", retryable: false.ToString());
    }

    [Fact]
    public async Task ApiThrows500_ReturnsFailedRetryable()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new ApiRequestException("internal error", 500) };
        var sender = CreateSender(EnabledOptions(), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Outcome.ShouldBe(NewsSendOutcome.Failed);
        result.Sent.ShouldBeFalse();
        result.Retryable.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("internal error");

        AssertNewsSendRecorded("failure", reason: "internal error", retryable: true.ToString());
    }

    [Fact]
    public async Task ApiSends_ReturnsSuccess()
    {
        var fake = new FakeTelegramBotApi();
        var sender = CreateSender(EnabledOptions(chatId: 100), fake);

        var result = await sender.Send(CreateNews(), "Мой проект", "my-project");

        result.Outcome.ShouldBe(NewsSendOutcome.Sent);
        result.Sent.ShouldBeTrue();
        result.Retryable.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
        fake.WasCalled.ShouldBeTrue();
        fake.CapturedChatId.ShouldBe(new TelegramChatId(100));
        fake.CapturedHtml.ShouldNotBeNull();
        fake.CapturedHtml.Contents.ShouldContain("<b>");

        AssertNewsSendRecorded("success");
        collector.Measurements.ShouldNotContain(m => m.Instrument == "telegram.sends"
            && Equals(m.Tags.GetValueOrDefault("kind"), "news")
            && Equals(m.Tags.GetValueOrDefault("result"), "success")
            && (m.Tags.ContainsKey("reason") || m.Tags.ContainsKey("retryable")));
    }

    public void Dispose() => collector.Dispose();
}
