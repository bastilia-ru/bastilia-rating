namespace Bastilia.Rating.Database;

internal class BastiliaMemberRepository(AppDbContext context) : BastiliaRepositoryBase, IBastiliaMemberRepository
{
    public async Task<BastiliaMember?> GetByIdAsync(int userId) => (await GetMemberImpl(u => u.JoinRpgUserId == userId)).FirstOrDefault();

    public async Task<BastiliaMember?> GetBySlugAsync(string slug) => (await GetMemberImpl(u => u.Slug == slug)).FirstOrDefault();

    public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => GetMemberImpl(u => true);

    private async Task<IReadOnlyCollection<BastiliaMember>> GetMemberImpl(Expression<Func<Entities.User, bool>> predicate)
    {
        var users = await context.Users
                    .Include(u => u.BastiliaStatuses)
                    .Include(u => u.ProjectAdmins)
                        .ThenInclude(pa => pa.Project)
                    .Include(u => u.Achievements)
                        .ThenInclude(a => a.Template)
                        .ThenInclude(a => a.Project)
                    .Include(u => u.Achievements)
                        .ThenInclude(a => a.GrantedByUser)
                    .Include(u => u.Achievements)
                        .ThenInclude(a => a.RemovedByUser)
                    .AsNoTracking()
                    .Where(predicate)
                    .ToArrayAsync();

        return [.. users.Select(ToMember)];
    }

    private static BastiliaMember ToMember(Entities.User user)
    {
        return new BastiliaMember(
            JoinrpgUserId: user.JoinRpgUserId,
            UserName: user.Username,
            AvatarUrl: user.AvatarUrl,
            Slug: user.Slug,
            ParticipateInRating: user.ParticipateInRating,
            StatusHistory: user.BastiliaStatuses
                .Select(s => new BastiliaStatusHistory(
                    s.BeginDate,
                    s.StatusType,
                    s.EndDate))
                .ToList()
                .AsReadOnly(),
            HisProjects: user.ProjectAdmins
                .Select(pa => new ProjectAdminInfo(
                    pa.ProjectId,
                    pa.Project.ProjectName,
                    pa.AddDate,
                    pa.RemoveDate))
                .ToList()
                .AsReadOnly(),
            Achievements: user.Achievements
                .Where(a => a.RemovedDate == null)
                .Select(a => new MemberAchievement(
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
                    new ProjectLink(a.Template.Project.BastiliaProjectId, a.Template.Project.ProjectName)
                    : null
                    ))
                .ToList()
                .AsReadOnly());
    }

    private record class ProjectLink(int BastiliaProjectId, string ProjectName) : IBastiliaProjectLink;
}