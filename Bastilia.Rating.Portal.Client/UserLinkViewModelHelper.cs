using Bastilia.Rating.Domain;
using JoinRpg.Common.PrimitiveTypes;
using JoinRpg.Common.WebComponents;

namespace Bastilia.Rating.Portal.Client;

public static class UserLinkViewModelHelper
{
    public static UserLinkViewModel ToViewModel(this IUserLink user)
    {
        if (user.JoinrpgUserId.Value <= 0)
        {
            return new UserLinkViewModel(null, user.UserName, ViewMode.Deleted);
        }

        return new UserLinkViewModel(user.JoinrpgUserId, user.UserName, ViewMode.Show, user.Slug);
    }
}
