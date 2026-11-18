using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Telegram.Bot.Types;

namespace Bastilia.Rating.Tests;

public class LoggingTelegramBotApiTests : IDisposable
{
    private readonly MeterListenerCollector collector = new(TelegramMetrics.MeterName);

    [Fact]
    public async Task SuccessfulCall_DelegatesAndRecordsSuccessMetric()
    {
        var fake = new FakeTelegramBotApi { UserName = "bastilia_bot" };
        var api = new LoggingTelegramBotApi(fake, NullLogger<LoggingTelegramBotApi>.Instance);

        var userName = await api.GetMyUserNameAsync();

        userName.ShouldBe("bastilia_bot");
        fake.GetMeCalls.ShouldBe(1);
        collector.Measurements.Any(m => m.Instrument == "telegram.api.duration"
            && Equals(m.Tags.GetValueOrDefault("method"), "GetMe")
            && Equals(m.Tags.GetValueOrDefault("result"), "success")).ShouldBeTrue();
    }

    [Fact]
    public async Task FailedGenericOverloadCall_PropagatesExceptionAndRecordsFailureMetric()
    {
        var fake = new FakeTelegramBotApi { GetMeException = new InvalidOperationException("proxy is dead") };
        var api = new LoggingTelegramBotApi(fake, NullLogger<LoggingTelegramBotApi>.Instance);

        await Should.ThrowAsync<InvalidOperationException>(() => api.GetMyUserNameAsync());

        collector.Measurements.Any(m => m.Instrument == "telegram.api.duration"
            && Equals(m.Tags.GetValueOrDefault("method"), "GetMe")
            && Equals(m.Tags.GetValueOrDefault("result"), nameof(InvalidOperationException))).ShouldBeTrue();
    }

    [Fact]
    public async Task FailedCall_PropagatesExceptionAndRecordsFailureMetric()
    {
        var fake = new FakeTelegramBotApi { GetUpdatesException = new InvalidOperationException("proxy is dead") };
        var api = new LoggingTelegramBotApi(fake, NullLogger<LoggingTelegramBotApi>.Instance);

        await Should.ThrowAsync<InvalidOperationException>(() => api.GetUpdatesAsync(offset: 3, TimeSpan.FromSeconds(25)));

        collector.Measurements.Any(m => m.Instrument == "telegram.api.duration"
            && Equals(m.Tags.GetValueOrDefault("method"), "GetUpdates")
            && Equals(m.Tags.GetValueOrDefault("result"), nameof(InvalidOperationException))).ShouldBeTrue();
    }

    [Fact]
    public async Task CallerCancellation_PropagatesWithoutFailureMetric()
    {
        var fake = new FakeTelegramBotApi { CancelGetMeByCaller = true };
        var api = new LoggingTelegramBotApi(fake, NullLogger<LoggingTelegramBotApi>.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var measurementsBefore = collector.Measurements.Count;

        await Should.ThrowAsync<OperationCanceledException>(() => api.GetMyUserNameAsync(cts.Token));

        // Штатная отмена вызывающим кодом — не поломка: метрика api.duration не пишется.
        // (Параллельные тестовые классы пишут свои измерения в тот же listener —
        // сравниваем только инструмент этого класса.)
        collector.Measurements.Skip(measurementsBefore)
            .Where(m => m.Instrument == "telegram.api.duration")
            .ShouldBeEmpty();
    }

    [Fact]
    public async Task HttpClientTimeout_RecordsFailureMetric()
    {
        // Таймаут HttpClient — TaskCanceledException, не связанный с токеном вызывающего:
        // главный симптом «прокси молчит», должен остаться в метриках и Warning-логе.
        var fake = new FakeTelegramBotApi { GetMeException = new TaskCanceledException() };
        var api = new LoggingTelegramBotApi(fake, NullLogger<LoggingTelegramBotApi>.Instance);

        await Should.ThrowAsync<TaskCanceledException>(() => api.GetMyUserNameAsync(CancellationToken.None));

        collector.Measurements.Any(m => m.Instrument == "telegram.api.duration"
            && Equals(m.Tags.GetValueOrDefault("method"), "GetMe")
            && Equals(m.Tags.GetValueOrDefault("result"), nameof(TaskCanceledException))).ShouldBeTrue();
    }

    public void Dispose() => collector.Dispose();

    private sealed class FakeTelegramBotApi : ITelegramBotApi
    {
        public int GetMeCalls { get; private set; }
        public string? UserName { get; set; }
        public Exception? GetMeException { get; set; }
        public Exception? GetUpdatesException { get; set; }
        public bool CancelGetMeByCaller { get; set; }

        public Task SendHtmlMessageAsync(TelegramChatId chatId, TelegramHtmlString html, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetMyUserNameAsync(CancellationToken cancellationToken = default)
        {
            GetMeCalls++;
            if (CancelGetMeByCaller)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return GetMeException is null
                ? Task.FromResult(UserName)
                : Task.FromException<string?>(GetMeException);
        }

        public Task SetWebhookAsync(string url, string secret, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Update>> GetUpdatesAsync(int offset, TimeSpan timeout, CancellationToken cancellationToken = default)
            => GetUpdatesException is null
                ? Task.FromResult<IReadOnlyList<Update>>([])
                : Task.FromException<IReadOnlyList<Update>>(GetUpdatesException);
    }
}
