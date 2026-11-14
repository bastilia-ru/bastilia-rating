using System.Security.Claims;
using Bastilia.Rating.Domain.DomainServices;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bastilia.Rating.Tests;

public class BastiliaUserLoginHandlerTests
{
    private const string NewNick = "nick";
    private const string NewAvatar = "https://a/64.png";

    private static BastiliaMember CreateMember(string userName = "old", string avatarUrl = "https://example.com/old.png") =>
        new(42, userName, avatarUrl, "slug", false, [], [], [], null, null);

    [Fact]
    public async Task NoUserInfoClaims_UsesLegacyXApiPath()
    {
        var db = new FakeUserDbService();
        var member = CreateMember();
        var handler = CreateHandler(member, db);
        var claims = new List<Claim>();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "42")]));

        await handler.HandleLoginAsync(new UserIdentification(42), principal, claims, CancellationToken.None);

        db.AddCalls.ShouldBeEmpty();
        db.UpdateCalls.ShouldBeEmpty();
        claims.ShouldContain(c => c.Type == ClaimTypes.Name && c.Value == member.UserName);
        claims.ShouldContain(c => c.Type == "avatar" && c.Value == member.AvatarUrl);
    }

    [Fact]
    public async Task UserInfoClaimsPresent_NewUser_ImportsWithClaims()
    {
        var db = new FakeUserDbService { MemberToReturn = CreateMember(userName: NewNick, avatarUrl: NewAvatar) };
        var handler = CreateHandler(existingMember: null, db);
        var claims = new List<Claim>();

        await handler.HandleLoginAsync(new UserIdentification(42), PrincipalWithUserInfoClaims(), claims, CancellationToken.None);

        db.AddCalls.ShouldBe([(42, NewNick, NewAvatar)]);
        db.UpdateCalls.ShouldBeEmpty();
        claims.ShouldContain(c => c.Type == ClaimTypes.Name && c.Value == NewNick);
        claims.ShouldContain(c => c.Type == "avatar" && c.Value == NewAvatar);
    }

    [Fact]
    public async Task UserInfoClaimsPresent_ExistingUser_UpdatesFromClaims()
    {
        var updated = CreateMember(userName: NewNick, avatarUrl: NewAvatar);
        var db = new FakeUserDbService { MemberToReturn = updated };
        var handler = CreateHandler(CreateMember(), db);
        var claims = new List<Claim>();

        await handler.HandleLoginAsync(new UserIdentification(42), PrincipalWithUserInfoClaims(), claims, CancellationToken.None);

        db.UpdateCalls.ShouldBe([(42, NewNick, NewAvatar)]);
        claims.ShouldContain(c => c.Type == ClaimTypes.Name && c.Value == NewNick);
    }

    private static ClaimsPrincipal PrincipalWithUserInfoClaims() =>
        new(new ClaimsIdentity(
        [
            new Claim("preferred_username", NewNick),
            new Claim("picture", NewAvatar),
        ]));

    private static Portal.Auth.BastiliaUserLoginHandler CreateHandler(BastiliaMember? existingMember, FakeUserDbService dbService)
    {
        var importService = new UserImportService(new FakeMemberRepository(existingMember), null!, dbService, NullLogger<UserImportService>.Instance);
        return new Portal.Auth.BastiliaUserLoginHandler(importService, NullLoggerFactory.Instance);
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
