namespace Bastilia.Rating.Tests;

public class ProjectNewsTests
{
    [Fact]
    public void NewsSendResult_Factories_SetFlagsAndMessages()
    {
        NewsSendResult.Success.ShouldBe(new NewsSendResult(true, false, null));
        NewsSendResult.Skipped("Telegram не настроен")
            .ShouldBe(new NewsSendResult(false, false, "Telegram не настроен"));
        NewsSendResult.Failure("bot was kicked", retryable: true)
            .ShouldBe(new NewsSendResult(false, true, "bot was kicked"));
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
