namespace Bastilia.Rating.Domain
{
    public record class BastiliaProjectWithDetails : BastiliaProject
    {
        public BastiliaProjectWithDetails(int BastiliaProjectId, string ProjectName, ProjectType ProjectType,
                                          BrandType BrandType, bool OngoingProject, bool? ProjectOfTheYear,
                                          int? JoinrpgProjectId, int? KogdaIgraProjectId, string? ProjectUri,
                                          IReadOnlyCollection<IUserLink> Coordinators,
                                          IReadOnlyCollection<MemberAchievement> ProjectMemberAchievements,
                                          IReadOnlyCollection<AchievementTemplate> Templates,
                                          DateOnly? endDate,
                                          string HowToHelp,
                                          string ProjectDescription,
                                          Uri ProjectIconUri,
                                          string? slug,
                                          string? password)
            : base(BastiliaProjectId, ProjectName, ProjectType, BrandType, OngoingProject, ProjectOfTheYear,
                   JoinrpgProjectId, KogdaIgraProjectId, ProjectUri, Coordinators, endDate, HowToHelp, ProjectIconUri, slug, password)
        {
            this.ProjectMemberAchievements = ProjectMemberAchievements;
            this.Templates = Templates;
            this.ProjectDescription = ProjectDescription;
        }

        public IReadOnlyCollection<MemberAchievement> ProjectMemberAchievements { get; }
        public IReadOnlyCollection<AchievementTemplate> Templates { get; }
        public string ProjectDescription { get; }
    }

}
