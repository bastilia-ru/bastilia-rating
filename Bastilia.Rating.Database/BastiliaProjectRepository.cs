namespace Bastilia.Rating.Database;

internal class BastiliaProjectRepository(AppDbContext context) : BastiliaRepositoryBase, IBastiliaProjectRepository
{
    public Task<BastiliaProjectWithDetails?> GetByIdAsync(int projectId) => GetOneProjectByPredicate(p => p.BastiliaProjectId == projectId);

    public Task<BastiliaProjectWithDetails?> GetBySlugAsync(string slug) => GetOneProjectByPredicate(p => p.Slug == slug);


    public Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects() => GetProjectsByPredicate(p => p.EndDate == null);

    public Task<IReadOnlyCollection<BastiliaProject>> GetProjectsWithoutPasswords() => GetProjectsByPredicate(p => p.Password == null);

    public Task<IReadOnlyCollection<BastiliaProject>> GetAllProjects() => GetProjectsByPredicate(p => true);

    public async Task<IReadOnlyCollection<BastiliaCalendarItem>> GetProjectCalendarFor(int year)
    {
        return [..Query()
            .Where(x => x.PlannedEndDate!.Value.Year == year || x.PlannedStartDate.Year == year)
            .Where(x => x.DeletedAt == null)
            .Select(x =>
            new BastiliaCalendarItem(BastiliaCalendarItemType.Project, x.PlannedStartDate, x.PlannedEndDate ?? x.PlannedStartDate, x.ProjectName))];
    }

    private async Task<IReadOnlyCollection<BastiliaProject>> GetProjectsByPredicate(Expression<Func<Entities.BastiliaProject, bool>> predicate)
    {
        var projects = await Query().Where(predicate).ToListAsync();

        return [.. projects.Select(ToProject)];
    }

    private IQueryable<Entities.BastiliaProject> Query()
    {
        return context.BastiliaProjects
                    .Include(bp => bp.ProjectAdmins)
                    .ThenInclude(pa => pa.User)
                    .AsNoTracking()
                    ;
    }


    private async Task<BastiliaProjectWithDetails?> GetOneProjectByPredicate(Expression<Func<Entities.BastiliaProject, bool>> predicate)
    {
        var project = await Query()
                    .Where(predicate)
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
           project.Password,
           project.DeletedAt,
           project.LastUpdatedAt
           );
    }
}
