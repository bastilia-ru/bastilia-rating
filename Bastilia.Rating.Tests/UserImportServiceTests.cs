using Bastilia.Rating.Domain.DomainServices;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bastilia.Rating.Tests;

public class UserImportServiceTests
{
    private static BastiliaMember CreateMember(string userName = "old", string avatarUrl = "https://example.com/old.png") =>
        new(1, userName, avatarUrl, "slug", true, [], [], [], null);

    [Fact]
    public async Task ExistingUser_NickChanged_UpdatesNickOnly()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(userName: "new") };
        var importService = CreateImportService(CreateMember(userName: "old"), db);

        var result = await importService.ImportUser(1, "new", null);

        db.UpdateCalls.ShouldBe([(1, "new", (string?)null)]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
        result.UserName.ShouldBe("new");
    }

    [Fact]
    public async Task ExistingUser_AvatarChangedOnly_UpdatesAvatarOnly()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(avatarUrl: "https://a/64.png") };
        var importService = CreateImportService(CreateMember(), db);

        var result = await importService.ImportUser(1, null, "https://a/64.png");

        db.UpdateCalls.ShouldBe([(1, (string?)null, "https://a/64.png")]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
        result.AvatarUrl.ShouldBe("https://a/64.png");
    }

    [Fact]
    public async Task ExistingUser_DataMatches_NoUpdate()
    {
        var db = new FakeUserDbService();
        var member = CreateMember(userName: "same", avatarUrl: "https://a/64.png");
        var importService = CreateImportService(member, db);

        var result = await importService.ImportUser(1, "same", "https://a/64.png");

        db.UpdateCalls.ShouldBeEmpty();
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(member);
    }

    [Fact]
    public async Task NewUser_NickOnly_CreatesWithEmptyAvatar()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(userName: "nick", avatarUrl: "") };
        var importService = CreateImportService(existingMember: null, db);

        var result = await importService.ImportUser(1, "nick", null);

        db.AddCalls.ShouldBe([(1, "nick", "")]);
        db.UpdateCalls.ShouldBeEmpty();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task NewUser_NickAndAvatar_CreatesWithBoth()
    {
        var created = CreateMember(userName: "nick", avatarUrl: "https://a/64.png");
        var db = new FakeUserDbService { MemberToReturn = created };
        var importService = CreateImportService(existingMember: null, db);

        var result = await importService.ImportUser(1, "nick", "https://a/64.png");

        db.AddCalls.ShouldBe([(1, "nick", "https://a/64.png")]);
        db.UpdateCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(created);
    }

    [Fact]
    public async Task ExistingUser_BothChanged_UpdatesBoth()
    {
        var updated = CreateMember(userName: "new", avatarUrl: "https://a/64.png");
        var db = new FakeUserDbService { MemberToReturn = updated };
        var importService = CreateImportService(CreateMember(), db);

        var result = await importService.ImportUser(1, "new", "https://a/64.png");

        db.UpdateCalls.ShouldBe([(1, "new", "https://a/64.png")]);
        db.AddCalls.ShouldBeEmpty();
        result.ShouldBeSameAs(updated);
    }

    private static UserImportService CreateImportService(BastiliaMember? existingMember, FakeUserDbService dbService)
        => new(new FakeMemberRepository(existingMember), null!, dbService, NullLogger<UserImportService>.Instance);

    private sealed class FakeMemberRepository(BastiliaMember? member) : IBastiliaMemberRepository
    {
        public Task<BastiliaMember?> GetByIdAsync(int userId) => Task.FromResult(member);

        public Task<BastiliaMember?> GetBySlugAsync(string slug) => throw new NotImplementedException();

        public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => throw new NotImplementedException();

        public Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync() => throw new NotImplementedException();

        public Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory() => throw new NotImplementedException();

        public Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year) => throw new NotImplementedException();
    }

    private sealed class FakeUserDbService : IUserDbService
    {
        public List<(int Id, string? Nick, string? Avatar)> UpdateCalls { get; } = [];
        public List<(int Id, string Nick, string Avatar)> AddCalls { get; } = [];

        public BastiliaMember? MemberToReturn { get; set; }

        public Task<BastiliaMember> AddUser(int playerId, string nickName, string avatarUrl)
        {
            AddCalls.Add((playerId, nickName, avatarUrl));
            return Task.FromResult(MemberToReturn ?? throw new InvalidOperationException("MemberToReturn не задан"));
        }

        public Task<BastiliaMember> UpdateUser(int playerId, string? nickName, string? avatarUrl)
        {
            UpdateCalls.Add((playerId, nickName, avatarUrl));
            return Task.FromResult(MemberToReturn ?? throw new InvalidOperationException("MemberToReturn не задан"));
        }
    }
}
