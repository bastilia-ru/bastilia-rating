using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebComponents;
using Microsoft.Extensions.DependencyInjection;

namespace Bastilia.Rating.Portal.Client;

public static class UserLinkRegistration
{
    public static IServiceCollection AddUserLinkLocator(this IServiceCollection services)
    {
        return services.AddTransient<IUriLocator<UserLinkViewModel>, UserLinkUriLocator>();
    }

    private sealed class UserLinkUriLocator : IUriLocator<UserLinkViewModel>
    {
        public Uri GetUri(UserLinkViewModel target)
        {
            if (target.Slug is not null)
            {
                return new Uri($"/member/{target.Slug}", UriKind.Relative);
            }

            if (target.UserId is { } userId)
            {
                return new Uri($"/member/{userId.Value}", UriKind.Relative);
            }

            throw new InvalidOperationException($"Не могу построить ссылку на профиль: нет ни slug, ни id (DisplayName={target.DisplayName}).");
        }
    }
}
