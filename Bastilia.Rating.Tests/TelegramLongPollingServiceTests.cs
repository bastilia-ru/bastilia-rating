using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Tests;

public class TelegramLongPollingServiceTests
{
    private static readonly JoinRpgHostNamesOptions HostNames = new()
    {
        MainHost = "joinrpg.ru",
        IdHost = "id.joinrpg.ru",
        KogdaIgraHost = "kogda-igra.ru",
        RatingHost = "rating.bastilia.ru",
    };

    [Fact]
    public async Task DisabledTelegram_DoesNotPoll()
    {
        var fake = new FakeTelegramBotApi();
        var service = CreateService(new TelegramBotOptions { BotId = 0, Transport = TelegramTransport.LongPolling }, fake);

        await RunService(service);

        fake.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task WebhookTransport_DoesNotPoll()
    {
        var fake = new FakeTelegramBotApi();
        var service = CreateService(new TelegramBotOptions { BotId = 42, BotSecret = "s" }, fake);

        await RunService(service);

        fake.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task LongPolling_SkipsPreStartUpdatesAndProcessesNewOnes()
    {
        var fake = new FakeTelegramBotApi();
        var service = CreateService(LongPollingOptions, fake);
        fake.EnqueueUpdates(CreateStartUpdate(updateId: 100, chatId: 1)); // апдейт до старта — игнорируем
        fake.EnqueueUpdates(CreateStartUpdate(updateId: 101, chatId: 555));
        fake.EnqueueCancellation();

        await RunService(service);

        fake.Calls.ShouldBe(["delete", "get:-1:0", "get:101:25", "send:555", "get:102:25"]);
        fake.CapturedChatIds.ShouldBe([new TelegramChatId(555)]);
    }

    [Fact]
    public async Task LongPolling_NoPreStartUpdates_StartsFromZeroOffset()
    {
        var fake = new FakeTelegramBotApi();
        var service = CreateService(LongPollingOptions, fake);
        fake.EnqueueUpdates(); // до старта апдейтов не было
        fake.EnqueueUpdates(CreateStartUpdate(updateId: 5, chatId: 777));
        fake.EnqueueCancellation();

        await RunService(service);

        fake.RequestedOffsets.ShouldBe([-1, 0, 6]);
        fake.CapturedChatIds.ShouldBe([new TelegramChatId(777)]);
    }

    [Fact]
    public async Task GetUpdatesThrows_RetriesAndContinues()
    {
        var fake = new FakeTelegramBotApi { ErrorRetryDelay = TimeSpan.FromMilliseconds(1) };
        var service = CreateService(LongPollingOptions, fake);
        fake.EnqueueUpdates();
        fake.EnqueueError(new HttpRequestException("connection failed"));
        fake.EnqueueUpdates(CreateStartUpdate(updateId: 9, chatId: 333));
        fake.EnqueueCancellation();

        await RunService(service);

        fake.RequestedOffsets.ShouldBe([-1, 0, 0, 10]);
        fake.CapturedChatIds.ShouldBe([new TelegramChatId(333)]);
    }

    [Fact]
    public async Task UpdateHandlingThrows_SkipsPoisonousUpdateAndAdvancesOffset()
    {
        var fake = new FakeTelegramBotApi();
        var service = CreateService(LongPollingOptions, fake, memberLookupExceptionForChat: 111);
        fake.EnqueueUpdates();
        fake.EnqueueUpdates(
            CreateStartUpdate(updateId: 1, chatId: 111),
            CreateStartUpdate(updateId: 2, chatId: 222));
        fake.EnqueueCancellation();

        await RunService(service);

        fake.RequestedOffsets.ShouldBe([-1, 0, 3]);
        fake.CapturedChatIds.ShouldBe([new TelegramChatId(222)]);
    }

    private static TelegramBotOptions LongPollingOptions => new() { BotId = 42, BotSecret = "s", Transport = TelegramTransport.LongPolling };

    private static async Task RunService(TelegramLongPollingService service)
    {
        await service.StartAsync(CancellationToken.None);
        // В .NET 10 StartAsync ставит ExecuteAsync в очередь пула потоков и возвращается
        // сразу; ждём именно задачу выполнения, иначе StopAsync ниже может отменить её
        // до старта, и тело не выполнится.
        if (service.ExecuteTask is { } executeTask)
        {
            await executeTask;
        }

        await service.StopAsync(CancellationToken.None);
    }

    private static TelegramLongPollingService CreateService(
        TelegramBotOptions options,
        FakeTelegramBotApi fake,
        long? memberLookupExceptionForChat = null)
    {
        var member = memberLookupExceptionForChat is null ? null : CreateMember("leo");
        var handler = new TelegramUpdateHandler(
            new StubMemberRepository(member, memberLookupExceptionForChat),
            fake,
            new WelcomeMessageBuilder(),
            Options.Create(HostNames),
            NullLogger<TelegramUpdateHandler>.Instance);
        return new TelegramLongPollingService(
            NullLogger<TelegramLongPollingService>.Instance,
            Options.Create(options),
            fake,
            handler)
        { ErrorRetryDelay = fake.ErrorRetryDelay };
    }

    private static Update CreateStartUpdate(long updateId, long chatId) => new()
    {
        Id = (int)updateId,
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

    private sealed class StubMemberRepository(BastiliaMember? member, long? exceptionForChat) : IBastiliaMemberRepository
    {
        public Task<BastiliaMember?> GetByIdAsync(int userId) => throw new NotImplementedException();
        public Task<BastiliaMember?> GetBySlugAsync(string slug) => throw new NotImplementedException();
        public Task<BastiliaMember?> GetByTelegramChatIdAsync(TelegramChatId chatId)
            => exceptionForChat is not null && chatId.Value == exceptionForChat
                ? throw new InvalidOperationException("member lookup failed")
                : Task.FromResult(member);
        public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year) => throw new NotImplementedException();
    }

    private sealed class FakeTelegramBotApi : ITelegramBotApi
    {
        private readonly Queue<object> getUpdatesResponses = new();

        public List<string> Calls { get; } = [];
        public List<int> RequestedOffsets { get; } = [];
        public List<TelegramChatId> CapturedChatIds { get; } = [];
        public TimeSpan ErrorRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

        public void EnqueueUpdates(params Update[] updates) => getUpdatesResponses.Enqueue((IReadOnlyList<Update>)updates);
        public void EnqueueError(Exception exception) => getUpdatesResponses.Enqueue(exception);
        public void EnqueueCancellation() => getUpdatesResponses.Enqueue(new OperationCanceledException());

        public Task<IReadOnlyList<Update>> GetUpdatesAsync(int offset, int timeoutSeconds, CancellationToken cancellationToken = default)
        {
            Calls.Add($"get:{offset}:{timeoutSeconds}");
            RequestedOffsets.Add(offset);
            var response = getUpdatesResponses.Dequeue();
            if (response is Exception exception)
            {
                throw exception;
            }

            return Task.FromResult((IReadOnlyList<Update>)response);
        }

        public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
        {
            Calls.Add($"send:{chatId.Value}");
            CapturedChatIds.Add(chatId);
            return Task.CompletedTask;
        }

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add("delete");
            return Task.CompletedTask;
        }
    }
}
