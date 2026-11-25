namespace Bastilia.Rating.Domain
{
    public record class BastiliaProjectWithDetails : BastiliaProject
    {
        public BastiliaProjectWithDetails(BastiliaProjectId BastiliaProjectId, string ProjectName, ProjectType ProjectType,
                                          BrandType BrandType, bool OngoingProject, bool? ProjectOfTheYear,
                                          int? JoinrpgProjectId, int? KogdaIgraProjectId, string? ProjectUri,
                                          IReadOnlyCollection<IUserLink> Coordinators,
                                          IReadOnlyCollection<MemberAchievement> ProjectMemberAchievements,
                                          IReadOnlyCollection<AchievementTemplate> Templates,
                                          DateOnly? endDate,
                                          ProjectLevel? projectLevel,
                                          string HowToHelp,
                                          string ProjectDescription,
                                          Uri ProjectIconUri,
                                          string? slug,
                                          DateTimeOffset? deletedAt,
                                          DateTimeOffset? lastUpdatedAt,
                                          DateOnly? plannedEndDate,
                                          DateTimeOffset? lastNewsDate,
                                          DateOnly createDate,
                                          NewsCadence newsCadence
                                          )
            : base(BastiliaProjectId, ProjectName, ProjectType, BrandType, OngoingProject, ProjectOfTheYear,
                   JoinrpgProjectId, KogdaIgraProjectId, ProjectUri, Coordinators, endDate, projectLevel, HowToHelp, ProjectIconUri, slug, deletedAt, lastUpdatedAt,
                   plannedEndDate, Templates.Count, ProjectMemberAchievements.Count, lastNewsDate, createDate, newsCadence)
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
