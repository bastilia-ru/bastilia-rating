using Bastilia.Rating.Domain;
using Microsoft.AspNetCore.Components;

namespace Bastilia.Rating.Portal.Common
{
    public class ProjectNavigateHelper(IBastiliaProjectRepository projectRepository, NavigationManager navigationManager)
    {
        public async Task<BastiliaProjectWithDetails?> LoadProjectWithCheck(string projectIdOrSlug)
        {
            BastiliaProjectWithDetails? project;
            if (int.TryParse(projectIdOrSlug, out var projectId))
            {
                project = await projectRepository.GetByIdAsync(projectId);
            }
            else
            {
                project = await projectRepository.GetBySlugAsync(projectIdOrSlug);
            }

            if (project is null)
            {
                navigationManager.NavigateTo("/404");
                return null;
            }
            else
            {
                return project;
            }
        }
    }
}
