using Bastilia.Rating.Portal.Common.Telegram;
using Telegram.Bot.Exceptions;

namespace Bastilia.Rating.Tests;

public class TelegramErrorClassifierTests
{
    [Fact]
    public void ApiError400_NotRetryable()
    {
        var (retryable, message) = TelegramErrorClassifier.Classify(new ApiRequestException("bad request", 400));

        retryable.ShouldBeFalse();
        message.ShouldBe("bad request");
    }

    [Fact]
    public void ApiError403_NotRetryable()
    {
        var (retryable, message) = TelegramErrorClassifier.Classify(new ApiRequestException("bot was kicked", 403));

        retryable.ShouldBeFalse();
        message.ShouldBe("bot was kicked");
    }

    [Fact]
    public void ApiError500_Retryable()
    {
        var (retryable, message) = TelegramErrorClassifier.Classify(new ApiRequestException("internal error", 500));

        retryable.ShouldBeTrue();
        message.ShouldBe("internal error");
    }

    [Fact]
    public void ApiError429_Retryable()
    {
        var (retryable, message) = TelegramErrorClassifier.Classify(new ApiRequestException("too many requests", 429));

        retryable.ShouldBeTrue();
        message.ShouldBe("too many requests");
    }

    [Fact]
    public void HttpRequestException_Retryable()
    {
        var (retryable, message) = TelegramErrorClassifier.Classify(new HttpRequestException("connection failed"));

        retryable.ShouldBeTrue();
        message.ShouldBe("connection failed");
    }

    [Fact]
    public void TimeoutException_Retryable()
    {
        var (retryable, message) = TelegramErrorClassifier.Classify(new TimeoutException("timed out"));

        retryable.ShouldBeTrue();
        message.ShouldBe("timed out");
    }

    [Fact]
    public void OtherException_Retryable()
    {
        var (retryable, message) = TelegramErrorClassifier.Classify(new InvalidOperationException("unexpected"));

        retryable.ShouldBeTrue();
        message.ShouldBe("unexpected");
    }
}
