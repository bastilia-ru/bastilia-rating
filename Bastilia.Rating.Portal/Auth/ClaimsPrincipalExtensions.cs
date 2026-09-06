using System.Security.Claims;
using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Portal.Auth;

internal static class ClaimsPrincipalExtensions
{
    // Не через extension-синтаксис (user.GetJoinrpgUserId()) — иначе имя совпадёт с этим же методом
    // и получится бесконечная рекурсия вместо вызова JoinRpg.Common.WebInfrastructure.Auth.ClaimsPrincipalExtensions.
    public static int GetJoinrpgUserId(this ClaimsPrincipal user)
        => JoinRpg.Common.WebInfrastructure.Auth.ClaimsPrincipalExtensions.GetJoinrpgUserId(user).Value;

    public static bool IsProjectAdmin(this ClaimsPrincipal user, BastiliaProject project)
    {
        return user.IsInRole(BastiliaRoles.President)
            || project.Coordinators.Any(c => c.JoinrpgUserId == user.GetJoinrpgUserId());
    }
}
