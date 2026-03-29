using System.Security.Claims;
using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Portal.Auth;

internal static class ClaimsPrincipalExtensions
{
    public static int GetJoinrpgUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : throw new InvalidOperationException("User has no valid JoinrpgUserId claim");
    }

    public static bool IsProjectAdmin(this ClaimsPrincipal user, BastiliaProject project)
    {
        return user.IsInRole(BastiliaRoles.President)
            || project.Coordinators.Any(c => c.JoinrpgUserId == user.GetJoinrpgUserId());
    }
}
