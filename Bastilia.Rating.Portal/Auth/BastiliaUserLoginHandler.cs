using System.Security.Claims;
using Bastilia.Rating.Domain;
using Bastilia.Rating.Domain.DomainServices;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebInfrastructure.Auth;

namespace Bastilia.Rating.Portal.Auth;

internal class BastiliaUserLoginHandler(UserImportService userImportService, ILoggerFactory loggerFactory) : IJoinUserLoginHandler
{
    public async Task HandleLoginAsync(UserIdentification userId, ClaimsPrincipal externalPrincipal, List<Claim> claims, CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Auth");

        var member = await userImportService.ImportUser(userId.Value);
        if (member is null)
        {
            logger.LogWarning("Не удалось загрузить пользователя {userId} с id.joinrpg.ru", userId);
            throw new InvalidOperationException($"Не удалось загрузить пользователя {userId} с id.joinrpg.ru");
        }

        claims.Add(new Claim(ClaimTypes.Name, member.UserName));
        claims.Add(new Claim("avatar", member.AvatarUrl));

        if (member.IsActiveMember)
        {
            claims.Add(new Claim(ClaimTypes.Role, BastiliaRoles.Member));
        }
        if (member.IsPresident)
        {
            claims.Add(new Claim(ClaimTypes.Role, BastiliaRoles.President));
        }
    }
}
