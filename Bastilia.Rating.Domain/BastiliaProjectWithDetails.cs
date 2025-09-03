namespace Bastilia.Rating.Domain
{
    public record class BastiliaProjectWithDetails : BastiliaProject
    {
        public BastiliaProjectWithDetails(int BastiliaProjectId, string ProjectName, ProjectType ProjectType,
                                          BrandType BrandType, bool OngoingProject, bool? ProjectOfTheYear,
                                          int? JoinrpgProjectId, int? KogdaIgraProjectId, string? ProjectUri,
                                          IReadOnlyCollection<IUserLink> Coordinators,
                                          IReadOnlyCollection<MemberAchievement> ProjectMemberAchievements,
                                          DateOnly? endDate,
                                          string HowToHelp,
                                          string ProjectDescription,
                                          Uri ProjectIconUri,
                                          string? slug)
            : base(BastiliaProjectId, ProjectName, ProjectType, BrandType, OngoingProject, ProjectOfTheYear,
                   JoinrpgProjectId, KogdaIgraProjectId, ProjectUri, Coordinators, endDate, HowToHelp, ProjectIconUri, slug)
        {
            this.ProjectMemberAchievements = ProjectMemberAchievements;
            this.ProjectDescription = ProjectDescription;
        }

        public IReadOnlyCollection<MemberAchievement> ProjectMemberAchievements { get; }
        public string ProjectDescription { get; }
    }

}
