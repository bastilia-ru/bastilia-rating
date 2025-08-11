using System.Diagnostics.CodeAnalysis;

namespace Bastilia.Rating.Database;

internal abstract class BastiliaRepositoryBase
{
    protected static BastiliaProject ToProject(Entities.BastiliaProject project)
    {
        return new BastiliaProject(
            project.BastiliaProjectId,
            project.ProjectName,
            project.ProjectType,
            project.BrandType,
            project.OngoingProject,
            project.ProjectOfTheYear,
            project.JoinrpgProjectId,
            project.KogdaIgraProjectId,
            project.ProjectUri,
            [.. project.ProjectAdmins.Select(pa => ToUserLink(pa.User))],
            project.EndDate is not null ? DateOnly.FromDateTime(project.EndDate.Value) : null
            );
    }

    [return: NotNullIfNotNull(nameof(user))]
    protected static IUserLink? ToUserLink(Entities.User? user)
    {
        if (user is null) { return null; }
        return new UserLink(user.JoinRpgUserId, user.Slug, user.Username);
    }

    private record UserLink(int JoinrpgUserId, string? Slug, string UserName) : IUserLink;
}