using System.Net;
using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Portal.Common.Telegram;

public class AchievementMessageBuilder
{
    public string BuildMessage(string memberName, AchievementTemplate template, string projectName, string? projectSlug, string ratingHost, bool participatesInRating)
    {
        var linkTarget = projectSlug ?? template.Project?.BastiliaProjectId.ToString()
            ?? throw new ArgumentException("Не удалось определить ссылку на проект: ни projectSlug, ни template.Project не заданы.", nameof(projectSlug));

        var greeting = $"Здравствуйте, <b>{WebUtility.HtmlEncode(memberName)}</b>!";
        var points = participatesInRating ? $"+{template.RatingValue} к рейтингу · " : string.Empty;
        var achievement = $"🎖 Вам выдана ачивка «<b>{WebUtility.HtmlEncode(template.Name)}</b>»\n{points}проект {WebUtility.HtmlEncode(projectName)}";
        var footer = $"<a href=\"https://{ratingHost}/project/{linkTarget}\">Подробности на сайте рейтинга</a>";

        return string.Join("\n\n", greeting, achievement, footer);
    }
}
