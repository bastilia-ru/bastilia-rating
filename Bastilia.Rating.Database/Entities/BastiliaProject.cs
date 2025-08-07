using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Database.Entities;

public class BastiliaProject
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public ProjectType ProjectType { get; set; }
    public BrandType BrandType { get; set; }
    public bool OngoingProject { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? ProjectOfTheYear { get; set; }

    public ICollection<ProjectAdmin> ProjectAdmins { get; set; }
    public ICollection<AchievementTemplate> AchievementTemplates { get; set; }
}
