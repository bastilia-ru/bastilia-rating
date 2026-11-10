using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;
using AchievementTemplate = Bastilia.Rating.Domain.AchievementTemplate;

namespace Bastilia.Rating.Tests;

public class AchievementGrantedNotifierTests : IDisposable
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

        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("bastilia_bot");
    }

    private static AchievementGrantedNotifier CreateNotifier(TelegramBotOptions telegramOptions, FakeTelegramBotApi fake) =>
        new(Options.Create(telegramOptions),
            Options.Create(new JoinRpgHostNamesOptions
            {
                MainHost = "joinrpg.ru",
                IdHost = "id.joinrpg.ru",
                KogdaIgraHost = "kogda-igra.ru",
                RatingHost = "rating.bastilia.ru",
            }),
            fake,
            new AchievementMessageBuilder(),
            NullLogger<AchievementGrantedNotifier>.Instance);

    private IReadOnlyDictionary<string, object?> FindAchievementSendMeasurement()
        => collector.Measurements
            .Single(m => m.Instrument == "telegram.sends" && Equals(m.Tags.GetValueOrDefault("kind"), "achievement"))
            .Tags;

    private static BastiliaMember CreateMember(TelegramSocialLink? telegram) =>
        new(JoinrpgUserId: 42, UserName: "Имя", AvatarUrl: "", Slug: null,
            ParticipateInRating: true, StatusHistory: [], HisProjects: [],
            Achievements: [], BirthDay: null, Telegram: telegram);

    private static readonly AchievementTemplate Template =
        new(Project: null, Name: "Ачивка", Description: "", DefaultUri: false,
            RatingValue: 5, YearlyAchievement: false, TemplateId: 7);

    private static TelegramBotOptions EnabledOptions() => new() { BotId = 42, BotSecret = "secret", ChatId = 100 };

    [Fact]
    public async Task TelegramDisabled_DoesNotSend()
    {
        var fake = new FakeTelegramBotApi();
        var notifier = CreateNotifier(new TelegramBotOptions { BotId = 0 }, fake);

        await notifier.NotifyAchievementGrantedAsync(CreateMember(TelegramSocialLink.FromOptional(123456789L, "leo")), Template, "Мой проект", "my-project");

        fake.WasCalled.ShouldBeFalse();

        var tags = FindAchievementSendMeasurement();
        tags["result"].ShouldBe("skipped");
        tags["reason"].ShouldBe("Telegram не настроен");
    }

    [Fact]
    public async Task MemberWithoutTelegram_DoesNotSend()
    {
        var fake = new FakeTelegramBotApi();
        var notifier = CreateNotifier(EnabledOptions(), fake);

        await notifier.NotifyAchievementGrantedAsync(CreateMember(null), Template, "Мой проект", "my-project");

        fake.WasCalled.ShouldBeFalse();

        var tags = FindAchievementSendMeasurement();
        tags["result"].ShouldBe("skipped");
        tags["reason"].ShouldBe("нет привязки Telegram");
    }

    [Fact]
    public async Task VerifiedMember_SendsToChatId()
    {
        var fake = new FakeTelegramBotApi();
        var notifier = CreateNotifier(EnabledOptions(), fake);

        await notifier.NotifyAchievementGrantedAsync(CreateMember(TelegramSocialLink.FromOptional(123456789L, "leo")), Template, "Мой проект", "my-project");

        fake.WasCalled.ShouldBeTrue();
        fake.CapturedChatId.ShouldBe(new TelegramChatId(123456789));
        fake.CapturedHtml.ShouldNotBeNull();
        fake.CapturedHtml.Contents.ShouldContain("Ачивка");

        var tags = FindAchievementSendMeasurement();
        tags["result"].ShouldBe("success");
        tags.ShouldNotContainKey("reason");
        tags.ShouldNotContainKey("retryable");
    }

    [Fact]
    public async Task ApiThrows403_NotifierDoesNotThrow()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new ApiRequestException("bot was blocked", 403) };
        var notifier = CreateNotifier(EnabledOptions(), fake);

        await notifier.NotifyAchievementGrantedAsync(CreateMember(TelegramSocialLink.FromOptional(123456789L, "leo")), Template, "Мой проект", "my-project");

        fake.WasCalled.ShouldBeTrue();

        var tags = FindAchievementSendMeasurement();
        tags["result"].ShouldBe("failure");
        tags["reason"].ShouldBe("bot was blocked");
        tags["retryable"].ShouldBe(false.ToString());
    }

    [Fact]
    public void PackageSemantics_FromOptionalWithChatId_IsNotVerifiedByDefault()
    {
        // FromOptional не считает ссылку верифицированной (флаг isVerified по умолчанию false),
        // поэтому нотификатор проверяет наличие chat id, а не IsVerified.
        var link = TelegramSocialLink.FromOptional(123456789L, "leo");

        link.ShouldNotBeNull();
        link.IsVerified.ShouldBeFalse();
        link.Id.ShouldBe(123456789L);
    }

    [Fact]
    public void PackageSemantics_FromOptionalWithoutChatId_ReturnsNull()
        => TelegramSocialLink.FromOptional((long?)null, "leo").ShouldBeNull();

    public void Dispose() => collector.Dispose();
}
