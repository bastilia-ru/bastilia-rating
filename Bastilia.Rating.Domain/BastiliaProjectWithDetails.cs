namespace Bastilia.Rating.Domain
{
    public record class BastiliaProjectWithDetails : BastiliaProject
    {
        public BastiliaProjectWithDetails(int BastiliaProjectId, string ProjectName, ProjectType ProjectType,
                                          BrandType BrandType, bool OngoingProject, bool? ProjectOfTheYear,
                                          int? JoinrpgProjectId, int? KogdaIgraProjectId, string? ProjectUri,
                                          IReadOnlyCollection<IUserLink> Coordinators,
                                          IReadOnlyCollection<ProjectMemberAchievement> ProjectMemberAchievements,
                                          DateOnly? endDate)
            : base(BastiliaProjectId, ProjectName, ProjectType, BrandType, OngoingProject, ProjectOfTheYear, JoinrpgProjectId, KogdaIgraProjectId, ProjectUri, Coordinators, endDate)
        {
            this.ProjectMemberAchievements = ProjectMemberAchievements;
        }

        public IReadOnlyCollection<ProjectMemberAchievement> ProjectMemberAchievements { get; }
    }

    public record class ProjectMemberAchievement(IUserLink User, string Name, int? Value);
}
