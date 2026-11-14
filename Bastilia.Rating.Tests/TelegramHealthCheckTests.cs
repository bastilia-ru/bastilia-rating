using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;

namespace Bastilia.Rating.Tests;

public class TelegramHealthCheckTests
{
    [Fact]
    public async Task DisabledTelegram_ReturnsDegraded()
    {
        var check = CreateCheck(new TelegramBotOptions { BotId = 0 }, new UnreachableBotApi());

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldBe("Telegram выключен");
    }

    [Fact]
    public async Task WorkingTelegram_ReturnsHealthyWithUserName()
    {
        var check = CreateCheck(EnabledOptions, new FakeTelegramBotApi { UserName = "bastilia_bot" });

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Description.ShouldBe("Подключен: bastilia_bot");
    }

    [Fact]
    public async Task ApiError_ReturnsUnhealthy()
    {
        var check = CreateCheck(EnabledOptions, new FakeTelegramBotApi
        {
            ExceptionToThrow = new ApiRequestException("bot was kicked", 403),
        });

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Description.ShouldNotBeNull().ShouldContain("403");
        result.Exception.ShouldBeOfType<ApiRequestException>();
    }

    [Fact]
    public async Task NetworkError_ReturnsUnhealthy()
    {
        var check = CreateCheck(EnabledOptions, new FakeTelegramBotApi
        {
            ExceptionToThrow = new HttpRequestException("Connection refused"),
        });

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Exception.ShouldBeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task Cancellation_PropagatesInsteadOfDiagnosis()
    {
        var check = CreateCheck(EnabledOptions, new FakeTelegramBotApi
        {
            ExceptionToThrow = new OperationCanceledException(),
        });

        await Should.ThrowAsync<OperationCanceledException>(() => check.CheckHealthAsync(new HealthCheckContext()));
    }

    private static TelegramBotOptions EnabledOptions => new() { BotId = 42, BotSecret = "secret", ChatId = 100 };

    private static TelegramHealthCheck CreateCheck(TelegramBotOptions options, ITelegramBotApi api)
        => new(Options.Create(options), new SingleServiceProvider(api), NullLogger<TelegramHealthCheck>.Instance);

    private sealed class FakeTelegramBotApi : ITelegramBotApi
    {
        public string? UserName { get; set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default)
            => ExceptionToThrow is null
                ? Task.FromResult(UserName)
                : Task.FromException<string?>(ExceptionToThrow);

        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Telegram.Bot.Types.Update>> GetUpdatesAsync(int offset, TimeSpan timeout, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    /// <summary>Ненастроенный Telegram не должен резолвить клиента вообще.</summary>
    private sealed class UnreachableBotApi : ITelegramBotApi
    {
        public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Ненастроенный Telegram не должен создавать клиент");

        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Ненастроенный Telegram не должен создавать клиент");

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Ненастроенный Telegram не должен создавать клиент");

        public Task<IReadOnlyList<Telegram.Bot.Types.Update>> GetUpdatesAsync(int offset, TimeSpan timeout, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Ненастроенный Telegram не должен создавать клиент");
    }

    private sealed class SingleServiceProvider(object service) : IServiceProvider
    {
        public object? GetService(Type serviceType) => serviceType.IsInstanceOfType(service) ? service : null;
    }
}
