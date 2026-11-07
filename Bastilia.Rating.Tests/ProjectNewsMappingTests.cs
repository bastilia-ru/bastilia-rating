namespace JoinRpg.PrimitiveTypes.Test;

public class ProjectNewsMappingTests
{
    [Fact]
    public void ToNews_MapsAllFields()
    {
        var createdAt = DateTimeOffset.Now;
        var entity = new ProjectNewsEntity
        {
            ProjectNewsId = 7,
            ProjectId = 55,
            AuthorUserId = 42,
            AuthorName = "Иванов",
            CreatedAt = createdAt,
            Text = "Новость проекта",
            SendStatus = NewsSendStatus.Sent,
            TelegramError = null,
        };

        var news = BastiliaNewsRepository.ToNews(entity);

        news.Id.ShouldBe(7);
        news.ProjectId.ShouldBe(55);
        news.AuthorUserId.ShouldBe(42);
        news.AuthorName.ShouldBe("Иванов");
        news.CreatedAt.ShouldBe(createdAt);
        news.Text.ShouldBe("Новость проекта");
        news.SendStatus.ShouldBe(NewsSendStatus.Sent);
        news.TelegramError.ShouldBeNull();
    }

    [Fact]
    public void ToNews_MapsTelegramError()
    {
        var entity = new ProjectNewsEntity
        {
            ProjectNewsId = 8,
            ProjectId = 56,
            AuthorUserId = 43,
            AuthorName = "Петров",
            CreatedAt = DateTimeOffset.Now,
            Text = "Ошибка отправки",
            SendStatus = NewsSendStatus.Failed,
            TelegramError = "chat not found",
        };

        var news = BastiliaNewsRepository.ToNews(entity);

        news.SendStatus.ShouldBe(NewsSendStatus.Failed);
        news.TelegramError.ShouldBe("chat not found");
    }
}
