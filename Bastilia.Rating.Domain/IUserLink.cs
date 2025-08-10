namespace Bastilia.Rating.Domain
{
    public interface IUserLink
    {
        int JoinrpgUserId { get; }
        string? Slug { get; }
        string UserName { get; }
    }
}