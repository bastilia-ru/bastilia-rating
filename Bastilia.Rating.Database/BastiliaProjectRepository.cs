namespace Bastilia.Rating.Database;

internal class BastiliaProjectRepository(AppDbContext context) : BastiliaRepositoryBase, IBastiliaProjectRepository
{
    public Task<BastiliaProjectWithDetails?> GetByIdAsync(int projectId) => GetOneProjectByPredicate(p => p.BastiliaProjectId == projectId);

    public Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug) => GetOneProjectByPredicate(p => p.Slug == slug);


    public Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects() => GetProjectsByPredicate(p => p.EndDate == null);

    public Task<IReadOnlyCollection<BastiliaProject>> GetProjectsWithoutPasswords() => GetProjectsByPredicate(p => p.Password == null);

    public Task<IReadOnlyCollection<BastiliaProject>> GetActualProjects() => GetProjectsByPredicate(p => true);

    public async Task<IReadOnlyCollection<BastiliaCalendarItem>> GetProjectCalendarFor(int year)
    {
        return [..Query(x => x.PlannedEndDate!.Value.Year == year || x.PlannedStartDate.Year == year).
            Select(x =>
            new BastiliaCalendarItem(BastiliaCalendarItemType.Project, x.PlannedStartDate, x.PlannedEndDate ?? x.PlannedStartDate, x.ProjectName))];
    }

    private async Task<IReadOnlyCollection<BastiliaProject>> GetProjectsByPredicate(Expression<Func<Entities.BastiliaProject, bool>> predicate)
    {
        var projects = await Query(predicate).ToListAsync();

        return [.. projects.Select(ToProject)];
    }

    private IQueryable<Entities.BastiliaProject> Query(Expression<Func<Entities.BastiliaProject, bool>> predicate)
    {
        return context.BastiliaProjects
                    .Include(bp => bp.ProjectAdmins)
                    .ThenInclude(pa => pa.User)
                    .AsNoTracking()
                    .Where(predicate)
                    ;
    }


    private async Task<BastiliaProjectWithDetails?> GetOneProjectByPredicate(Expression<Func<Entities.BastiliaProject, bool>> predicate)
    {
        var project = await Query(predicate)
                    .Include(p => p.AchievementTemplates)
                    .ThenInclude(p => p.Achievements)
                    .ThenInclude(p => p.User)
                    .Include(p => p.AchievementTemplates).ThenInclude(at => at.Achievements).ThenInclude(a => a.GrantedByUser)
                    .Include(p => p.AchievementTemplates).ThenInclude(at => at.Achievements).ThenInclude(a => a.RemovedByUser)
                    .FirstOrDefaultAsync();

        if (project == null) { return null; }


        return new BastiliaProjectWithDetails(
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
           [.. project.AchievementTemplates.SelectMany(a => a.Achievements).Select(ToMemberAchievement)],
           [.. project.AchievementTemplates.Select(ToTemplate)],
           project.EndDate,
           project.HowToHelp,
           project.ProjectDescription,
           new Uri(project.ProjectIconUri),
           project.Slug,
           project.Password
           );
    }
}
