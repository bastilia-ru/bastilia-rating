using System.Diagnostics.CodeAnalysis;

namespace Bastilia.Rating.Database;

internal abstract class BastiliaRepositoryBase
{
    protected static string GetAchievementName(Entities.Achievement a)
    {
        if (a.OverrideName is not null)
        {
            return a.OverrideName;
        }
        if (a.Template.YearlyAchievement)
        {
            return a.Template.AchievementName + " " + a.GrantedDate.Year;
        }
        return a.Template.AchievementName;
    }
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
            project.EndDate,
            project.HowToHelp,
            new Uri(project.ProjectIconUri),
            project.Slug
            );
    }

    [return: NotNullIfNotNull(nameof(user))]
    protected static IUserLink? ToUserLink(Entities.User? user)
    {
        if (user is null) { return null; }
        return new UserLink(user.JoinRpgUserId, user.Slug, user.Username);
    }

    protected static MemberAchievement ToMemberAchievement(Entities.Achievement a)
    {
        return new MemberAchievement(
                            GetAchievementName(a),
                            a.Template.AchievementDescription,
                            new Uri(a.Template.AchievementImageUrl),
                            a.Template.AchievementRatingValue,
                            a.GrantedDate,
                            ToUserLink(a.GrantedByUser),
                            a.RemovedDate,
                            ToUserLink(a.RemovedByUser),
                            a.ExpirationDate,
                            a.Template.ProjectId is not null ?
                            ToProjectLink(a.Template.Project)
                            : null,
                            a.User.ParticipateInRating,
                            ToUserLink(a.User),
                            new Uri(a.User.AvatarUrl)
                            );
    }

    protected static IBastiliaProjectLink ToProjectLink(Entities.BastiliaProject project)
    {
        return new ProjectLink(project.BastiliaProjectId, project.ProjectName, project.Slug);
    }

    private record class ProjectLink(int BastiliaProjectId, string ProjectName, string? Slug) : IBastiliaProjectLink;

    private record class UserLink(int JoinrpgUserId, string? Slug, string UserName) : IUserLink;
}