using System.Security.Claims;
using Bastilia.Rating.Portal.Auth;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Tests;

public class SiteAdminAuthorizationHandlerTests
{
    [Fact]
    public async Task UserIdInList_Succeeds()
    {
        var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "42")], "test")));

        await CreateHandler(adminUserIds: [42]).HandleAsync(context);

        context.HasSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task PresidentNotInList_Fails()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "2"), new Claim(ClaimTypes.Role, "president")], "test"));
        var context = CreateContext(principal);

        await CreateHandler(adminUserIds: [1]).HandleAsync(context);

        context.HasSucceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task Anonymous_Fails()
    {
        var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity()));

        await CreateHandler(adminUserIds: [1]).HandleAsync(context);

        context.HasSucceeded.ShouldBeFalse();
    }

    [Fact]
    public void Options_BindFromConfiguration()
    {
        ResolveAdminUserIds(new Dictionary<string, string?> { ["SiteAdmin:AdminUserIds:0"] = "42" })
            .ShouldBe([new UserIdentification(42)]);
        ResolveAdminUserIds([])
            .ShouldBeEmpty();
    }

    private static IReadOnlyCollection<UserIdentification> ResolveAdminUserIds(Dictionary<string, string?> config)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSiteAdminAuthorization(configuration);
        return services.BuildServiceProvider().GetRequiredService<IOptions<SiteAdminOptions>>().Value.AdminUserIds;
    }

    private static SiteAdminAuthorizationHandler CreateHandler(int[] adminUserIds)
        => new(new StubOptionsMonitor(new SiteAdminOptions { AdminUserIds = [.. adminUserIds.Select(id => new UserIdentification(id))] }));

    private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal principal)
        => new([new SiteAdminRequirement()], principal, null);

    private sealed class StubOptionsMonitor(SiteAdminOptions options) : IOptionsMonitor<SiteAdminOptions>
    {
        public SiteAdminOptions CurrentValue => options;
        public SiteAdminOptions Get(string? name) => options;
        public IDisposable? OnChange(Action<SiteAdminOptions, string?> listener) => null;
    }
}
