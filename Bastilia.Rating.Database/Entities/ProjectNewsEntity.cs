using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Database.Entities;

public class ProjectNewsEntity
{
    public int ProjectNewsId { get; set; }
    public int ProjectId { get; set; }
    public BastiliaProject Project { get; set; } = null!;
    public int AuthorUserId { get; set; }
    public string AuthorName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public string Text { get; set; } = "";
    public NewsSendStatus SendStatus { get; set; }
    public string? TelegramError { get; set; }
}
