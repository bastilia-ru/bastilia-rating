using Bastilia.Rating.Portal.Common.Telegram;

namespace Bastilia.Rating.Tests;

public class TelegramMetricsTests : IDisposable
{
    private readonly MeterListenerCollector collector = new(TelegramMetrics.MeterName);

    [Fact]
    public void SendSucceeded_RecordsSendWithSuccessTag()
    {
        TelegramMetrics.SendSucceeded("test-kind-success");

        var measurement = collector.Measurements.Single(m => m.Instrument == "telegram.sends");
        measurement.Tags["kind"].ShouldBe("test-kind-success");
        measurement.Tags["result"].ShouldBe("success");
        measurement.Tags.ShouldNotContainKey("reason");
        measurement.Tags.ShouldNotContainKey("retryable");
    }

    [Fact]
    public void SendFailed_RecordsSendWithFailureReasonAndRetryable()
    {
        TelegramMetrics.SendFailed("test-kind-failure", "bot was blocked", retryable: false);

        var measurement = collector.Measurements.Single(m => m.Instrument == "telegram.sends");
        measurement.Tags["kind"].ShouldBe("test-kind-failure");
        measurement.Tags["result"].ShouldBe("failure");
        measurement.Tags["reason"].ShouldBe("bot was blocked");
        measurement.Tags["retryable"].ShouldBe(false.ToString());
    }

    [Fact]
    public void SendSkipped_RecordsSendWithSkippedReason()
    {
        TelegramMetrics.SendSkipped("test-kind-skipped", "нет привязки");

        var measurement = collector.Measurements.Single(m => m.Instrument == "telegram.sends");
        measurement.Tags["kind"].ShouldBe("test-kind-skipped");
        measurement.Tags["result"].ShouldBe("skipped");
        measurement.Tags["reason"].ShouldBe("нет привязки");
        measurement.Tags.ShouldNotContainKey("retryable");
    }

    [Fact]
    public void WebhookProcessed_RecordsProcessedWebhook()
    {
        TelegramMetrics.WebhookProcessed();

        var measurement = collector.Measurements.Single(m => m.Instrument == "telegram.webhooks");
        measurement.Tags["result"].ShouldBe("processed");
        measurement.Tags.ShouldNotContainKey("reason");
    }

    [Fact]
    public void WebhookSkipped_RecordsSkippedWebhookWithReason()
    {
        TelegramMetrics.WebhookSkipped("invalid_secret");

        var measurement = collector.Measurements.Single(m => m.Instrument == "telegram.webhooks");
        measurement.Tags["result"].ShouldBe("skipped");
        measurement.Tags["reason"].ShouldBe("invalid_secret");
    }

    public void Dispose() => collector.Dispose();
}
