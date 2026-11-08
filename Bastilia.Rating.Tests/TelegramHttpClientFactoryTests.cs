using Bastilia.Rating.Portal.Common.Telegram;

namespace Bastilia.Rating.Tests;

public class TelegramHttpClientFactoryTests
{
    [Fact]
    public void CreateWithoutProxy_ReturnsNull()
    {
        TelegramHttpClientFactory.Create(null).ShouldBeNull();
    }

    [Fact]
    public void CreateWithProxy_ReturnsClient()
    {
        var client = TelegramHttpClientFactory.Create(new TelegramProxyOptions { Address = "http://proxy:8080" });

        client.ShouldNotBeNull();
    }

    [Fact]
    public void CreateWithProxyAndCredentials_ReturnsClient()
    {
        var client = TelegramHttpClientFactory.Create(new TelegramProxyOptions
        {
            Address = "socks5://proxy:1080",
            Username = "user",
            Password = "pass",
        });

        client.ShouldNotBeNull();
    }
}
