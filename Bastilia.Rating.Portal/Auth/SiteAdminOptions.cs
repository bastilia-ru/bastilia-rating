using Bastilia.Rating.Domain.Common;

namespace Bastilia.Rating.Portal.Auth;

public class SiteAdminOptions
{
    public UserIdentification[] AdminUserIds { get; set; } = [];
}
