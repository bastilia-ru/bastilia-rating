using LinqKit;

namespace Bastilia.Rating.Database;

internal class BastiliaMemberRepository(AppDbContext context) : BastiliaRepositoryBase, IBastiliaMemberRepository
{
    public async Task<BastiliaMember?> GetByIdAsync(int userId) => (await GetMemberImpl(u => u.JoinRpgUserId == userId)).FirstOrDefault();

    public async Task<BastiliaMember?> GetBySlugAsync(string slug) => (await GetMemberImpl(u => u.Slug == slug)).FirstOrDefault();

    public Task<IReadOnlyCollection<BastiliaMember>> GetAllAsync() => GetMemberImpl(u => true);

    public async Task<IReadOnlyCollection<BastiliaMember>> GetActualAsync()
    {
        var actualStatusPredicate = GetActualStatusPredicate();
        var users = await MemberQuery()
                    .AsNoTracking()
                    .Where(u => actualStatusPredicate.Invoke(u))
                    .ToArrayAsync();

        return [.. users.Select(ToMember).Where(x => x.CurrentStatus == BastiliaFinalStatus.Active)];
    }

    public async Task<IReadOnlyCollection<MemberHistoryItem>> GetMembersHistory()
    {
        var users = await context.UsersBastiliaStatuses
                    .Include(s => s.User)
                    .Where(s => s.StatusType == BastiliaStatusType.Member)
                    .ToArrayAsync();
        return [.. users.Select(s => new MemberHistoryItem(ToUserLink(s.User), s.BeginDate, s.EndDate))];
    }

    public async Task<IReadOnlyCollection<BastiliaCalendarItem>> GetMemberCalendarFor(int year)
    {
        var actualStatusPredicate = GetActualStatusPredicate();
        var birthdays = await MemberQuery()
            .AsNoTracking()
            .Where(u => actualStatusPredicate.Invoke(u))
            .Where(u => u.BirthDay != null)
            .Select(u => new { u.Username, BirthDay = u.BirthDay!.Value })
            .ToArrayAsync();

        var parties = await MemberQuery()
            .SelectMany(m => m.UserBirthdayParties)
            .Select(mbp => new { mbp.User.Username, mbp.PartyDate })
            .Where(p => p.PartyDate.Year == year)
            .ToArrayAsync();

        return [
            ..birthdays.Select(b=> new BastiliaCalendarItem(BastiliaCalendarItemType.Birthday, new DateOnly(year, b.BirthDay.Month, b.BirthDay.Day),  b.Username)),
            ..parties.Select(b => new BastiliaCalendarItem(BastiliaCalendarItemType.BirthdayParty, b.PartyDate, b.Username))
            ];
    }

    private async Task<IReadOnlyCollection<BastiliaMember>> GetMemberImpl(Expression<Func<Entities.User, bool>> predicate)
    {

        var actualStatusPredicate = GetActualStatusPredicate();
        var users = await MemberQuery()
                    .AsNoTracking()
                    .Select(u => new { User = u, CurrentStatus = actualStatusPredicate.Invoke(u) })
                    .Where(u => predicate.Invoke(u.User))
                    .ToArrayAsync();

        return [.. users.Select(u => ToMember(u.User))];
    }

    private static Expression<Func<Entities.User, bool>> GetActualStatusPredicate()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return u => u.BastiliaStatuses.Where(s => s.EndDate == null || s.EndDate > today).Where(s => s.BeginDate < today).OrderBy(s => s.BeginDate).Any();
    }

    private IQueryable<Entities.User> MemberQuery()
    {
        return context.Users
                            .AsExpandable()
                            .Include(u => u.BastiliaStatuses)
                            .Include(u => u.ProjectAdmins)
                                .ThenInclude(pa => pa.Project)
                            .Include(u => u.Achievements)
                                .ThenInclude(a => a.Template)
                                .ThenInclude(a => a.Project)
                            .Include(u => u.Achievements)
                                .ThenInclude(a => a.GrantedByUser)
                            .Include(u => u.Achievements)
                                .ThenInclude(a => a.RemovedByUser);
    }

    private static BastiliaMember ToMember(Entities.User user)
    {
        IUserLink userAsLink = ToUserLink(user);
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
                .Select(a => ToMemberAchievement(a))
                .ToList()
                .AsReadOnly(),
            BirthDay: user.BirthDay);
    }
}