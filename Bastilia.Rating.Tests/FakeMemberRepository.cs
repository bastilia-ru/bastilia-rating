namespace Bastilia.Rating.Tests;

internal sealed class FakeMemberRepository(BastiliaMember? member = null, long? chatIdToThrow = null) : IBastiliaMemberRepository
{
    public Task<BastiliaMember?> GetByIdAsync(int userId) => Task.FromResult(member);

    public Task<BastiliaMember?> GetBySlugAsync(string slug) => throw new NotImplementedException();

    public Task<BastiliaMember?> GetByTelegramChatIdAsync(TelegramChatId chatId)
        => chatIdToThrow is not null && chatId.Value == chatIdToThrow
            ? throw new InvalidOperationException("member lookup failed")
            : Task.FromResult(member);

    public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => throw new NotImplementedException();

    public Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync() => throw new NotImplementedException();

    public Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory() => throw new NotImplementedException();

    public Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year) => throw new NotImplementedException();
}
