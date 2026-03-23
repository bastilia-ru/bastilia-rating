using Bastilia.Rating.Database;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Bastilia.Rating.Portal.Auth;

internal static class AuthRegistration
{
    internal static void AddJoinRpgAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
            });

        services.AddCascadingAuthenticationState();

        var oidcOptions = configuration.GetSection("JoinRpgOidc").Get<JoinRpgOidcOptions>()
            ?? throw new InvalidOperationException("JoinRpgOidc configuration section is required");

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>();
            })
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough();

                options.UseSystemNetHttp();

                options.AddRegistration(new OpenIddict.Client.OpenIddictClientRegistration
                {
                    Issuer = oidcOptions.Issuer,
                    ClientId = oidcOptions.ClientId,
                    ClientSecret = oidcOptions.ClientSecret,
                    Scopes = { "openid", "profile" },
                    RedirectUri = new Uri("/signin-joinrpg", UriKind.Relative),
                });
            });

        services.AddAuthorization();
    }
}
