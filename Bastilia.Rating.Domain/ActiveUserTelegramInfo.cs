using JoinRpg.Common.PrimitiveTypes;

namespace Bastilia.Rating.Domain;

public record ActiveUserTelegramInfo(
    UserIdentification UserId,
    string? Slug,
    string UserName,
    TelegramSocialLink? Telegram) : IUserLink
{
    public int JoinrpgUserId => UserId.Value;
}
