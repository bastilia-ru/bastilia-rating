using Bastilia.Rating.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Auth;

internal static class SiteAdminAuthRegistration
{
    public static IServiceCollection AddSiteAdminAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SiteAdminOptions>()
            .BindConfiguration("SiteAdmin")
            .PostConfigure(o => o.AdminUserIds = o.AdminUserIds is [] ? [new UserIdentification(1)] : o.AdminUserIds);
        services.AddAuthorization(o => o.AddPolicy(SiteAdminPolicy.Name, p => p.Requirements.Add(new SiteAdminRequirement())));
        services.AddSingleton<IAuthorizationHandler, SiteAdminAuthorizationHandler>();
        return services;
    }
}
