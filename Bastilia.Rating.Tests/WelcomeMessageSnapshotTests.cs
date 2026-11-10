using Bastilia.Rating.Portal.Common.Telegram;
using VerifyXunit;

namespace Bastilia.Rating.Tests;

public class WelcomeMessageSnapshotTests : VerifyBase
{
    public WelcomeMessageSnapshotTests() : base()
    {
    }

    [Fact]
    public Task BuildMessage_KnownMember_IncludesProfileLink()
    {
        var member = new BastiliaMember(
            JoinrpgUserId: 42, UserName: "Имя & Фамилия", AvatarUrl: "",
            Slug: "leo", ParticipateInRating: true,
            StatusHistory: [], HisProjects: [], Achievements: [],
            BirthDay: null, Telegram: null);

        var message = new WelcomeMessageBuilder().BuildMessage(member, "rating.bastilia.ru");

        return Verify(message);
    }

    [Fact]
    public Task BuildMessage_UnknownMember_ProducesWelcomeWithoutLink()
    {
        var message = new WelcomeMessageBuilder().BuildMessage(null, "rating.bastilia.ru");

        return Verify(message);
    }
}
