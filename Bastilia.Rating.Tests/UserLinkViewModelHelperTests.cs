using Bastilia.Rating.Portal.Client;
using JoinRpg.Common.WebComponents;
using Microsoft.Extensions.DependencyInjection;

namespace JoinRpg.PrimitiveTypes.Test;

public class UserLinkViewModelHelperTests
{
    [Fact]
    public void ToViewModel_WithSlug_PrefersSlug()
    {
        var model = new TestUserLink(new UserIdentification(42), "ivanov", "Иван").ToViewModel();

        model.ViewMode.ShouldBe(ViewMode.Show);
        model.UserId.ShouldBe(new UserIdentification(42));
        model.Slug.ShouldBe("ivanov");
        CreateLocator().GetUri(model).ToString().ShouldBe("/member/ivanov");
    }

    [Fact]
    public void ToViewModel_WithoutSlug_BuildsUriById()
    {
        var model = new TestUserLink(new UserIdentification(42), null, "Иван").ToViewModel();

        model.ViewMode.ShouldBe(ViewMode.Show);
        model.Slug.ShouldBeNull();
        CreateLocator().GetUri(model).ToString().ShouldBe("/member/42");
    }

    [Fact]
    public void ToViewModel_PseudoUser_BecomesDeleted()
    {
        var model = new TestUserLink(new UserIdentification(-1), null, "Йорик").ToViewModel();

        model.ViewMode.ShouldBe(ViewMode.Deleted);
        model.UserId.ShouldBeNull();
        model.DisplayName.ShouldBe("Йорик");
    }

    private static IUriLocator<UserLinkViewModel> CreateLocator()
    {
        using var provider = new ServiceCollection().AddUserLinkLocator().BuildServiceProvider();
        return provider.GetRequiredService<IUriLocator<UserLinkViewModel>>();
    }

    private sealed record TestUserLink(UserIdentification JoinrpgUserId, string? Slug, string UserName) : IUserLink;
}
