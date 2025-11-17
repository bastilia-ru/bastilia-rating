using Bastilia.Rating.Domain;
using Bastilia.Rating.Domain.DomainServices;
using Microsoft.AspNetCore.Components;

namespace Bastilia.Rating.Portal.Common
{
    public class UserLoaderHelper(IBastiliaMemberRepository bastiliaMemberRepository, NavigationManager navigationManager, UserImportService userImportService)
    {
        public async Task<BastiliaMember?> LoadUserWithCheck(string userIdOrSlug)
        {
            BastiliaMember? user;
            if (int.TryParse(userIdOrSlug, out var userId))
            {
                user = await bastiliaMemberRepository.GetByIdAsync(userId);
                if (user is null)
                {
                    user = await userImportService.ImportUser(userId);
                }
            }
            else
            {
                user = await bastiliaMemberRepository.GetBySlugAsync(userIdOrSlug);
            }

            if (user is null)
            {
                navigationManager.NavigateTo("/404");
                return null;
            }
            else
            {
                return user;
            }
        }
    }
}
