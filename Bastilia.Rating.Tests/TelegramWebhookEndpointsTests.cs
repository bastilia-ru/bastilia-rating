using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Tests;

public class TelegramWebhookEndpointsTests : IDisposable
{
    private const string WebhookSecret = "topsecret";
    private const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";

    private readonly MeterListenerCollector collector = new(TelegramMetrics.MeterName);

    private static readonly JoinRpgHostNamesOptions HostNames = new()
    {
        MainHost = "joinrpg.ru",
        IdHost = "id.joinrpg.ru",
        KogdaIgraHost = "kogda-igra.ru",
        RatingHost = "rating.bastilia.ru",
    };

    private static readonly TelegramBotOptions OptionsWithSecret = new()
    {
        BotId = 42,
        BotSecret = "secret",
        ChatId = 100,
        WebhookSecret = WebhookSecret,
    };

    [Fact]
    public async Task InvalidSecret_ReturnsUnauthorized_AndRecordsSkipped()
    {
        var fake = new FakeTelegramBotApi();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[SecretHeader] = "wrong-secret";

        var result = await TelegramWebhookEndpoints.HandleWebhookAsync(
            CreateStartUpdate(123), CreateHandler(fake), Options.Create(OptionsWithSecret), httpContext, CancellationToken.None);

        result.ShouldBeOfType<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>();
        fake.WasCalled.ShouldBeFalse();

        collector.Measurements.ShouldContain(m => m.Instrument == "telegram.webhooks"
            && Equals(m.Tags.GetValueOrDefault("result"), "skipped")
            && Equals(m.Tags.GetValueOrDefault("reason"), "invalid_secret"));
    }

    [Fact]
    public async Task ValidStartCommand_ReturnsOk_AndRecordsProcessed()
    {
        var fake = new FakeTelegramBotApi();
        var httpContext = CreateAuthorizedHttpContext();

        var result = await TelegramWebhookEndpoints.HandleWebhookAsync(
            CreateStartUpdate(123), CreateHandler(fake), Options.Create(OptionsWithSecret), httpContext, CancellationToken.None);

        result.ShouldBeOfType<Microsoft.AspNetCore.Http.HttpResults.Ok>();
        fake.WasCalled.ShouldBeTrue();

        collector.Measurements.ShouldContain(m => m.Instrument == "telegram.webhooks"
            && Equals(m.Tags.GetValueOrDefault("result"), "processed")
            && !m.Tags.ContainsKey("reason"));
    }

    [Fact]
    public async Task ValidSecret_SendFails_ReturnsOk_AndStillRecordsProcessed()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new ApiRequestException("bot was blocked", 403) };
        var httpContext = CreateAuthorizedHttpContext();

        var result = await TelegramWebhookEndpoints.HandleWebhookAsync(
            CreateStartUpdate(123), CreateHandler(fake), Options.Create(OptionsWithSecret), httpContext, CancellationToken.None);

        result.ShouldBeOfType<Microsoft.AspNetCore.Http.HttpResults.Ok>();

        collector.Measurements.ShouldContain(m => m.Instrument == "telegram.webhooks"
            && Equals(m.Tags.GetValueOrDefault("result"), "processed"));
        collector.Measurements.ShouldContain(m => m.Instrument == "telegram.sends"
            && Equals(m.Tags.GetValueOrDefault("kind"), "welcome")
            && Equals(m.Tags.GetValueOrDefault("result"), "failure")
            && Equals(m.Tags.GetValueOrDefault("reason"), "bot was blocked"));
    }

    private static DefaultHttpContext CreateAuthorizedHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[SecretHeader] = WebhookSecret;
        return httpContext;
    }

    private static Update CreateStartUpdate(long chatId) => new()
    {
        Message = new Message
        {
            Chat = new Chat { Id = chatId },
            Text = "/start",
        },
    };

    private static TelegramUpdateHandler CreateHandler(FakeTelegramBotApi fake)
        => new(new StubMemberRepository(), fake, new WelcomeMessageBuilder(), Options.Create(HostNames), NullLogger<TelegramUpdateHandler>.Instance);

    private sealed class StubMemberRepository : IBastiliaMemberRepository
    {
        public Task<BastiliaMember?> GetByIdAsync(int userId) => throw new NotImplementedException();
        public Task<BastiliaMember?> GetBySlugAsync(string slug) => throw new NotImplementedException();
        public Task<BastiliaMember?> GetByTelegramChatIdAsync(TelegramChatId chatId) => Task.FromResult<BastiliaMember?>(null);
        public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year) => throw new NotImplementedException();
    }

    private sealed class FakeTelegramBotApi : ITelegramBotApi
    {
        public bool WasCalled { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.CompletedTask;
        }

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<Update>> GetUpdatesAsync(int offset, TimeSpan timeout, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    public void Dispose() => collector.Dispose();
}
