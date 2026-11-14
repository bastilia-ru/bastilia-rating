namespace Bastilia.Rating.Domain;

public record ActiveUserTelegramInfo(
    int JoinrpgUserId,
    string? Slug,
    string UserName,
    long? TelegramChatId,
    string? TelegramUsername) : IUserLink;
