using System.Net;
using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Portal.Common.Telegram;

public class WelcomeMessageBuilder
{
    private const string Greeting =
        "Привет! Я надзиратель бастильской тюрьмы. Я буду уведомлять тебя, если случилось что-то интересное — например, ты получил ачивку в рейтинге.";

    public string BuildMessage(BastiliaMember? member, string ratingHost)
    {
        if (member is null)
        {
            var unknown = "К сожалению, я пока не знаю, кто ты. Если ты член клуба, укажи свой Telegram в профиле на сайте рейтинга — тогда я смогу приветствовать тебя по имени.";
            return string.Join("\n\n", Greeting, unknown);
        }

        var profileLink = member.Slug ?? member.JoinrpgUserId.ToString();
        var greeting = $"Здравствуйте, <b>{WebUtility.HtmlEncode(member.UserName)}</b>!";
        var profile = $"<a href=\"https://{ratingHost}/member/{profileLink}\">Твой профиль в рейтинге</a>";

        return string.Join("\n\n", greeting, Greeting, profile);
    }
}
