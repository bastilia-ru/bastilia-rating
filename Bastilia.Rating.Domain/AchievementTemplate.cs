namespace Bastilia.Rating.Domain
{
    public record class AchievementTemplate(IBastiliaProjectLink? Project, string Name, string Description, bool DefaultUri, int RatingValue, bool YearlyAchievement, int TemplateId);
}