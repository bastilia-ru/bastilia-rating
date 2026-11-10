using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Tests;

public class TelegramUpdateHandlerTests : IDisposable
{
    private readonly MeterListenerCollector collector = new(TelegramMetrics.MeterName);

    private static readonly JoinRpgHostNamesOptions HostNames = new()
    {
        MainHost = "joinrpg.ru",
        IdHost = "id.joinrpg.ru",
        KogdaIgraHost = "kogda-igra.ru",
        RatingHost = "rating.bastilia.ru",
    };

    [Fact]
    public async Task StartCommand_KnownMember_SendsWelcomeWithProfileLink()
    {
        var fake = new FakeTelegramBotApi();
        var handler = CreateHandler(fake, CreateMember(slug: "leo"));

        await handler.HandleStartCommandAsync(CreateStartUpdate(123));

        fake.WasCalled.ShouldBeTrue();
        fake.CapturedChatId.ShouldBe(new TelegramChatId(123));
        fake.CapturedHtml!.Contents.ShouldContain("https://rating.bastilia.ru/member/leo");

        AssertWelcomeSendRecorded("success");
        collector.Measurements.ShouldNotContain(m => m.Instrument == "telegram.sends"
            && Equals(m.Tags.GetValueOrDefault("kind"), "welcome")
            && Equals(m.Tags.GetValueOrDefault("result"), "success")
            && (m.Tags.ContainsKey("reason") || m.Tags.ContainsKey("retryable")));
    }

    [Fact]
    public async Task StartCommand_UnknownMember_SendsWelcomeWithoutLink()
    {
        var fake = new FakeTelegramBotApi();
        var handler = CreateHandler(fake, member: null);

        await handler.HandleStartCommandAsync(CreateStartUpdate(999));

        fake.WasCalled.ShouldBeTrue();
        fake.CapturedChatId.ShouldBe(new TelegramChatId(999));
        fake.CapturedHtml!.Contents.ShouldNotContain("/member/");

        AssertWelcomeSendRecorded("success");
        collector.Measurements.ShouldNotContain(m => m.Instrument == "telegram.sends"
            && Equals(m.Tags.GetValueOrDefault("kind"), "welcome")
            && Equals(m.Tags.GetValueOrDefault("result"), "success")
            && (m.Tags.ContainsKey("reason") || m.Tags.ContainsKey("retryable")));
    }

    [Fact]
    public async Task StartCommandWithBotMention_IsHandled()
    {
        var fake = new FakeTelegramBotApi();
        var handler = CreateHandler(fake, CreateMember(slug: "leo"));
        var update = CreateStartUpdate(123);
        update.Message!.Text = "/start@bastilia_bot";

        await handler.HandleStartCommandAsync(update);

        fake.WasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task PlainText_DoesNotSend()
    {
        var fake = new FakeTelegramBotApi();
        var handler = CreateHandler(fake, CreateMember(slug: "leo"));
        var update = CreateStartUpdate(123);
        update.Message!.Text = "привет";

        await handler.HandleStartCommandAsync(update);

        fake.WasCalled.ShouldBeFalse();

        collector.Measurements.ShouldContain(m => m.Instrument == "telegram.webhooks"
            && Equals(m.Tags.GetValueOrDefault("result"), "skipped")
            && Equals(m.Tags.GetValueOrDefault("reason"), "not_start_command"));
    }

    [Fact]
    public async Task NonMessageUpdate_DoesNotSend()
    {
        var fake = new FakeTelegramBotApi();
        var handler = CreateHandler(fake, CreateMember(slug: "leo"));

        await handler.HandleStartCommandAsync(new Update());

        fake.WasCalled.ShouldBeFalse();

        collector.Measurements.ShouldContain(m => m.Instrument == "telegram.webhooks"
            && Equals(m.Tags.GetValueOrDefault("result"), "skipped")
            && Equals(m.Tags.GetValueOrDefault("reason"), "not_start_command"));
    }

    [Fact]
    public async Task SendFailure_DoesNotThrow()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new Telegram.Bot.Exceptions.ApiRequestException("bot was blocked", 403) };
        var handler = CreateHandler(fake, CreateMember(slug: "leo"));

        await handler.HandleStartCommandAsync(CreateStartUpdate(123));

        AssertWelcomeSendRecorded("failure", reason: "bot was blocked", retryable: false.ToString());
    }

    private void AssertWelcomeSendRecorded(string result, string? reason = null, string? retryable = null)
        => collector.Measurements.ShouldContain(m => m.Instrument == "telegram.sends"
            && Equals(m.Tags.GetValueOrDefault("kind"), "welcome")
            && Equals(m.Tags.GetValueOrDefault("result"), result)
            && (reason == null || Equals(m.Tags.GetValueOrDefault("reason"), reason))
            && (retryable == null || Equals(m.Tags.GetValueOrDefault("retryable"), retryable)));

    private static Update CreateStartUpdate(long chatId) => new()
    {
        Message = new Message
        {
            Chat = new Chat { Id = chatId },
            Text = "/start",
        },
    };

    private static BastiliaMember CreateMember(string slug) =>
        new(JoinrpgUserId: 42, UserName: "Имя", AvatarUrl: "", Slug: slug,
            ParticipateInRating: true, StatusHistory: [], HisProjects: [], Achievements: [],
            BirthDay: null, Telegram: null);

    private static TelegramUpdateHandler CreateHandler(FakeTelegramBotApi fake, BastiliaMember? member)
        => new(new StubMemberRepository(member), fake, new WelcomeMessageBuilder(), Options.Create(HostNames), NullLogger<TelegramUpdateHandler>.Instance);

    private sealed class StubMemberRepository(BastiliaMember? member) : IBastiliaMemberRepository
    {
        public Task<BastiliaMember?> GetByIdAsync(int userId) => throw new NotImplementedException();
        public Task<BastiliaMember?> GetBySlugAsync(string slug) => throw new NotImplementedException();
        public Task<BastiliaMember?> GetByTelegramChatIdAsync(TelegramChatId chatId) => Task.FromResult(member);
        public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year) => throw new NotImplementedException();
    }

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

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    public void Dispose() => collector.Dispose();
}
