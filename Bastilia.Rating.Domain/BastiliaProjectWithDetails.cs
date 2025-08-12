namespace Bastilia.Rating.Domain
{
    public record class BastiliaProjectWithDetails : BastiliaProject
    {
        public BastiliaProjectWithDetails(int BastiliaProjectId, string ProjectName, ProjectType ProjectType,
                                          BrandType BrandType, bool OngoingProject, bool? ProjectOfTheYear,
                                          int? JoinrpgProjectId, int? KogdaIgraProjectId, string? ProjectUri,
                                          IReadOnlyCollection<IUserLink> Coordinators,
                                          IReadOnlyCollection<ProjectMemberAchievement> ProjectMemberAchievements,
                                          DateOnly? endDate,
                                          string HowToHelp,
                                          string ProjectDescription)
            : base(BastiliaProjectId, ProjectName, ProjectType, BrandType, OngoingProject, ProjectOfTheYear,
                   JoinrpgProjectId, KogdaIgraProjectId, ProjectUri, Coordinators, endDate, HowToHelp)
        {
            this.ProjectMemberAchievements = ProjectMemberAchievements;
            this.ProjectDescription = ProjectDescription;
        }

        public IReadOnlyCollection<ProjectMemberAchievement> ProjectMemberAchievements { get; }
        public string ProjectDescription { get; }
    }

    public record class ProjectMemberAchievement(IUserLink User, string Name, int? Value);
}
