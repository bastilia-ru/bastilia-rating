using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Tests;

public class TelegramWebhookRegistrarTests
{
    private static readonly JoinRpgHostNamesOptions HostNames = new()
    {
        MainHost = "joinrpg.ru",
        IdHost = "id.joinrpg.ru",
        KogdaIgraHost = "kogda-igra.ru",
        RatingHost = "rating.bastilia.ru",
    };

    [Fact]
    public async Task DisabledTelegram_DoesNotCallApi()
    {
        var fake = new FakeTelegramBotApi();
        var registrar = CreateRegistrar(new TelegramBotOptions { BotId = 0, WebhookSecret = "secret" }, fake);

        await RunRegistrar(registrar);

        fake.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnabledWithoutSecret_DoesNotCallApi()
    {
        var fake = new FakeTelegramBotApi();
        var registrar = CreateRegistrar(new TelegramBotOptions { BotId = 42, BotSecret = "s" }, fake);

        await RunRegistrar(registrar);

        fake.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnabledWithSecret_CallsDeleteThenSetWithExpectedUrl()
    {
        var fake = new FakeTelegramBotApi();
        var registrar = CreateRegistrar(new TelegramBotOptions { BotId = 42, BotSecret = "s", WebhookSecret = "secret" }, fake);

        await RunRegistrar(registrar);

        fake.Calls.ShouldBe(["delete", "set:https://rating.bastilia.ru/api/telegram/webhook:secret"]);
    }

    [Fact]
    public async Task ApiThrows_DoesNotPropagate()
    {
        var fake = new FakeTelegramBotApi { ExceptionToThrow = new HttpRequestException("connection failed") };
        var registrar = CreateRegistrar(new TelegramBotOptions { BotId = 42, BotSecret = "s", WebhookSecret = "secret" }, fake);

        await RunRegistrar(registrar);
    }

    private static async Task RunRegistrar(TelegramWebhookRegistrar registrar)
    {
        await registrar.StartAsync(CancellationToken.None);
        // В .NET 10 StartAsync ставит ExecuteAsync в очередь пула потоков и возвращается
        // сразу; ждём именно задачу выполнения, иначе StopAsync ниже может отменить её
        // до старта, и тело не выполнится.
        if (registrar.ExecuteTask is { } executeTask)
        {
            await executeTask;
        }

        await registrar.StopAsync(CancellationToken.None);
    }

    private static TelegramWebhookRegistrar CreateRegistrar(TelegramBotOptions options, FakeTelegramBotApi fake)
        => new(NullLogger<TelegramWebhookRegistrar>.Instance, Options.Create(options), Options.Create(HostNames), fake);

    private sealed class FakeTelegramBotApi : ITelegramBotApi
    {
        public List<string> Calls { get; } = [];
        public Exception? ExceptionToThrow { get; set; }

        public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Telegram.Bot.Types.Update>> GetUpdatesAsync(int offset, int timeoutSeconds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
            Calls.Add($"set:{url}:{secret}");
            return Task.CompletedTask;
        }

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }
            Calls.Add("delete");
            return Task.CompletedTask;
        }
    }
}
