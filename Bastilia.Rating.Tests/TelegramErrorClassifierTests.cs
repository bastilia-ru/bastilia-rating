using Bastilia.Rating.Portal.Common.Telegram;
using Telegram.Bot.Exceptions;

namespace Bastilia.Rating.Tests;

public class TelegramErrorClassifierTests
{
    [Fact]
    public void ApiError400_NotRetryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new ApiRequestException("bad request", 400));

        retryable.ShouldBeFalse();
        category.ShouldBe("bad_request");
        message.ShouldBe("bad request");
    }

    [Fact]
    public void ApiError403_NotRetryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new ApiRequestException("bot was kicked", 403));

        retryable.ShouldBeFalse();
        category.ShouldBe("forbidden");
        message.ShouldBe("bot was kicked");
    }

    [Fact]
    public void ApiError500_Retryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new ApiRequestException("internal error", 500));

        retryable.ShouldBeTrue();
        category.ShouldBe("api_error");
        message.ShouldBe("internal error");
    }

    [Fact]
    public void ApiError429_Retryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new ApiRequestException("too many requests", 429));

        retryable.ShouldBeTrue();
        category.ShouldBe("api_error");
        message.ShouldBe("too many requests");
    }

    [Fact]
    public void RequestException_Retryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new RequestException("network down"));

        retryable.ShouldBeTrue();
        category.ShouldBe("request_failed");
        message.ShouldBe("network down");
    }

    [Fact]
    public void HttpRequestException_Retryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new HttpRequestException("connection failed"));

        retryable.ShouldBeTrue();
        category.ShouldBe("http_error");
        message.ShouldBe("connection failed");
    }

    [Fact]
    public void TimeoutException_Retryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new TimeoutException("timed out"));

        retryable.ShouldBeTrue();
        category.ShouldBe("timeout");
        message.ShouldBe("timed out");
    }

    [Fact]
    public void OtherException_Retryable()
    {
        var (retryable, category, message) = TelegramErrorClassifier.Classify(new InvalidOperationException("unexpected"));

        retryable.ShouldBeTrue();
        category.ShouldBe("unknown");
        message.ShouldBe("unexpected");
    }
}
