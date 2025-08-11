namespace Bastilia.Rating.Database;

internal class BastiliaProjectRepository(AppDbContext context) : BastiliaRepositoryBase, IBastiliaProjectRepository
{
    public async Task<BastiliaProjectWithDetails?> GetByIdAsync(int projectId)
    {
        var project = await Query(p => p.BastiliaProjectId == projectId)
            .Include(p => p.AchievementTemplates)
            .ThenInclude(p => p.Achievements)
            .ThenInclude(p => p.User)
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
           [.. project.AchievementTemplates.SelectMany(a => a.Achievements).Select(ToPma)],
           project.EndDate is not null ? DateOnly.FromDateTime(project.EndDate.Value) : null
           );
    }

    public Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects() => GetProjectsByPredicate(p => p.EndDate == null);

    public Task<IReadOnlyCollection<BastiliaProject>> GetActualProjects() => GetProjectsByPredicate(p => p.EndDate == null || p.EndDate > DateTime.UtcNow.AddYears(-2));

    private static ProjectMemberAchievement ToPma(Entities.Achievement a)
    {
        return new ProjectMemberAchievement(ToUserLink(a.User), a.OverrideName ?? a.Template.AchievementName, a.User.ParticipateInRating ? a.Template.AchievementRatingValue : null);
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
}
