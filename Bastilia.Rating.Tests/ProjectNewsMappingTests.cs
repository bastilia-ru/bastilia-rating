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
            ProjectId = new BastiliaProjectId(55),
            AuthorUserId = new UserIdentification(42),
            Author = new User { JoinRpgUserId = new UserIdentification(42), Username = "Иванов", AvatarUrl = "https://example.com/a.png" },
            CreatedAt = createdAt,
            Text = "Новость проекта",
            SendStatus = NewsSendStatus.Sent,
            TelegramError = null,
        };

        var news = BastiliaNewsRepository.ToNews(entity);

        news.Id.ShouldBe(7);
        news.ProjectId.ShouldBe(new BastiliaProjectId(55));
        news.AuthorUserId.ShouldBe(new UserIdentification(42));
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
            ProjectId = new BastiliaProjectId(56),
            AuthorUserId = new UserIdentification(43),
            Author = new User { JoinRpgUserId = new UserIdentification(43), Username = "Петров", AvatarUrl = "https://example.com/b.png" },
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
