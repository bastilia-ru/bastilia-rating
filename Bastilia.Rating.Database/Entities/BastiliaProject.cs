namespace Bastilia.Rating.Database.Entities;

public class BastiliaProject
{
    public int BastiliaProjectId { get; set; }
    public required string ProjectName { get; set; }
    public ProjectType ProjectType { get; set; }
    public BrandType BrandType { get; set; }
    public bool OngoingProject { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? ProjectOfTheYear { get; set; }

    public int? JoinrpgProjectId { get; set; }
    public int? KogdaIgraProjectId { get; set; }
    public string? ProjectUri { get; set; }

    public ICollection<ProjectAdmin> ProjectAdmins { get; set; } = [];
    public ICollection<AchievementTemplate> AchievementTemplates { get; set; } = [];
}
