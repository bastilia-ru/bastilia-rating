namespace Bastilia.Rating.Database;

internal class BastiliaNewsRepository(IDbContextFactory<AppDbContext> contextFactory) : BastiliaRepositoryBase, IProjectNewsRepository
{
    public async Task<ProjectNews?> GetLastForProject(int projectId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var entity = await Query(context)
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
        return entity is null ? null : ToNews(entity);
    }

    public async Task<IReadOnlyCollection<ProjectNews>> GetAllForProject(int projectId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var entities = await Query(context)
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return [.. entities.Select(ToNews)];
    }

    private static IQueryable<Entities.ProjectNewsEntity> Query(AppDbContext context)
    {
        return context.ProjectNews.AsNoTracking()
            .Include(x => x.Author);
    }

    internal static ProjectNews ToNews(Entities.ProjectNewsEntity entity)
    {
        return new ProjectNews(
            entity.ProjectNewsId,
            entity.ProjectId,
            entity.AuthorUserId,
            entity.Author.Username,
            entity.CreatedAt,
            entity.Text,
            entity.SendStatus,
            entity.TelegramError);
    }
}
