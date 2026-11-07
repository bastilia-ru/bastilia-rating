namespace Bastilia.Rating.Tests;

public class ProjectNewsTests
{
    [Fact]
    public void NewsSendResult_Factories_SetOutcomeFlagsAndMessages()
    {
        NewsSendResult.Success.Outcome.ShouldBe(NewsSendOutcome.Sent);
        NewsSendResult.Success.Sent.ShouldBeTrue();
        NewsSendResult.Success.Retryable.ShouldBeFalse();
        NewsSendResult.Success.ErrorMessage.ShouldBeNull();

        var skipped = NewsSendResult.Skipped("Telegram не настроен");
        skipped.Outcome.ShouldBe(NewsSendOutcome.Skipped);
        skipped.Sent.ShouldBeFalse();
        skipped.Retryable.ShouldBeFalse();
        skipped.ErrorMessage.ShouldBe("Telegram не настроен");

        var failure = NewsSendResult.Failure("bot was kicked", retryable: true);
        failure.Outcome.ShouldBe(NewsSendOutcome.Failed);
        failure.Sent.ShouldBeFalse();
        failure.Retryable.ShouldBeTrue();
        failure.ErrorMessage.ShouldBe("bot was kicked");

        NewsSendResult.Skipped("same text").ShouldNotBe(NewsSendResult.Failure("same text", retryable: false));
    }

    [Fact]
    public void ProjectNews_Record_RoundTrips()
    {
        var created = DateTimeOffset.UtcNow;
        var news = new ProjectNews(1, 2, 3, "Имя Автора", created, "**текст**", NewsSendStatus.Failed, "chat not found");
        news.Id.ShouldBe(1);
        news.ProjectId.ShouldBe(2);
        news.SendStatus.ShouldBe(NewsSendStatus.Failed);
        news.TelegramError.ShouldBe("chat not found");
    }
}
