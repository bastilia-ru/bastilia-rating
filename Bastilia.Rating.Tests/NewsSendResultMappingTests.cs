using Bastilia.Rating.Database.DbServices;

namespace Bastilia.Rating.Tests;

public class NewsSendResultMappingTests
{
    private static ProjectNewsEntity CreateEntity() => new()
    {
        ProjectNewsId = 1,
        ProjectId = 2,
        AuthorUserId = 3,
        AuthorName = "Имя Автора",
        CreatedAt = DateTimeOffset.UtcNow,
        Text = "**текст**",
        SendStatus = NewsSendStatus.NotSent,
        TelegramError = null,
    };

    [Fact]
    public void Sent_SetsSentStatusAndClearsError()
    {
        var entity = CreateEntity();
        entity.TelegramError = "старая ошибка";

        NewsService.ApplySendResult(entity, NewsSendResult.Success);

        entity.SendStatus.ShouldBe(NewsSendStatus.Sent);
        entity.TelegramError.ShouldBeNull();
    }

    [Fact]
    public void Skipped_KeepsNotSentStatusAndStoresReason()
    {
        var entity = CreateEntity();

        NewsService.ApplySendResult(entity, NewsSendResult.Skipped("Telegram не настроен"));

        entity.SendStatus.ShouldBe(NewsSendStatus.NotSent);
        entity.TelegramError.ShouldBe("Telegram не настроен");
    }

    [Fact]
    public void FailedNonRetryable_SetsFailedStatusAndStoresError()
    {
        var entity = CreateEntity();

        NewsService.ApplySendResult(entity, NewsSendResult.Failure("bot was kicked", retryable: false));

        entity.SendStatus.ShouldBe(NewsSendStatus.Failed);
        entity.TelegramError.ShouldBe("bot was kicked");
    }

    [Fact]
    public void FailedRetryable_SetsFailedStatusAndStoresError()
    {
        var entity = CreateEntity();

        NewsService.ApplySendResult(entity, NewsSendResult.Failure("internal error", retryable: true));

        entity.SendStatus.ShouldBe(NewsSendStatus.Failed);
        entity.TelegramError.ShouldBe("internal error");
    }
}
