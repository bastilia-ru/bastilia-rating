namespace Bastilia.Rating.Database;

internal class BastiliaProjectRepository(AppDbContext context) : IBastiliaProjectRepository
{
    public async Task<BastiliaProject?> GetByIdAsync(int projectId)
    {
        var project = await Query(p => p.BastiliaProjectId == projectId).FirstOrDefaultAsync();

        if (project == null) { return null; }


        return ToProject(project);
    }

    public Task<IReadOnlyCollection<BastiliaProject>> GetActiveProjects() => GetProjectsByPredicate(p => p.EndDate == null);

    public Task<IReadOnlyCollection<BastiliaProject>> GetActualProjects() => GetProjectsByPredicate(p => p.EndDate == null || p.EndDate > DateTime.UtcNow.AddYears(-2));

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

    private static BastiliaProject ToProject(Entities.BastiliaProject project)
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
            [.. project.ProjectAdmins.Select(pa => new UserLink(pa.User.JoinRpgUserId, pa.User.Slug, pa.User.Username))]
            );
    }

    private record UserLink(int JoinrpgUserId, string? Slug, string UserName) : IUserLink;
}
