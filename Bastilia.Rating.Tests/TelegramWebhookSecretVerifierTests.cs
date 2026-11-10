using Bastilia.Rating.Portal.Common.Telegram;

namespace Bastilia.Rating.Tests;

public class TelegramWebhookSecretVerifierTests
{
    [Fact]
    public void ValidSecret_Matches() => TelegramWebhookSecretVerifier.IsValid("secret", "secret").ShouldBeTrue();

    [Fact]
    public void WrongSecret_Rejected() => TelegramWebhookSecretVerifier.IsValid("secret", "other").ShouldBeFalse();

    [Theory]
    [InlineData("", "secret")]
    [InlineData("", null)]
    [InlineData(null, "secret")]
    [InlineData("secret", null)]
    public void MissingSecret_Rejected(string? configured, string? provided)
        => TelegramWebhookSecretVerifier.IsValid(configured, provided).ShouldBeFalse();
}
