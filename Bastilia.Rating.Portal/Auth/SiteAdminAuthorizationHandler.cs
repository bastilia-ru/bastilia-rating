using JoinRpg.Common.WebInfrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Auth;

internal sealed class SiteAdminRequirement : IAuthorizationRequirement;

internal sealed class SiteAdminAuthorizationHandler(IOptionsMonitor<SiteAdminOptions> options) : AuthorizationHandler<SiteAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SiteAdminRequirement requirement)
    {
        var userId = context.User.TryGetJoinrpgUserId();
        if (userId is not null && options.CurrentValue.AdminUserIds.Any(a => a.Value == userId.Value))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
