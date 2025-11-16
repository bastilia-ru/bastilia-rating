namespace Bastilia.Rating.Database.DbServices
{
    internal class ProjectService(AppDbContext appDbContext) : IProjectService
    {
        public async Task EnsureProjectHasPassword(int projectId)
        {
            var entity = await appDbContext.Set<Entities.BastiliaProject>().FindAsync(projectId) ?? throw new InvalidOperationException();
            if (entity.Password == null)
            {
                entity.Password = SlugGenerator.Generate();
                await appDbContext.SaveChangesAsync();
            }
        }
    }
}
