using Bastilia.Rating.Portal.Common.Telegram;
using Microsoft.Extensions.Logging;

namespace Bastilia.Rating.Tests;

public class TelegramOptionsLoggerTests
{
    [Fact]
    public void PostConfigure_LogsEffectiveOptionsAndMasksSecrets()
    {
        var logger = new ListLogger();
        var options = new TelegramBotOptions
        {
            BotId = 42,
            BotSecret = "super-secret-token",
            ChatId = -100,
            Proxy = new TelegramProxyOptions
            {
                Address = "https://proxy.example:8443",
                Username = "proxy-user",
                Password = "proxy-password",
            },
        };
        var postConfigure = new TelegramOptionsLogger(logger);

        postConfigure.PostConfigure(name: null, options);

        var message = logger.Messages.ShouldHaveSingleItem();
        message.ShouldContain("enabled=True");
        message.ShouldContain("botId=42");
        message.ShouldContain("chatId=-100");
        message.ShouldContain("https://proxy.example:8443");
        // Секреты не попадают в лог — только их наличие.
        message.ShouldNotContain("super-secret-token");
        message.ShouldNotContain("proxy-password");
        message.ShouldContain("user=задан");
        message.ShouldContain("password=задан");
    }

    [Fact]
    public void PostConfigure_WithoutProxy_LogsDirectConnection()
    {
        var logger = new ListLogger();
        var postConfigure = new TelegramOptionsLogger(logger);

        postConfigure.PostConfigure(name: null, new TelegramBotOptions { BotId = 0 });

        logger.Messages.ShouldHaveSingleItem().ShouldContain("прямое подключение");
    }

    [Fact]
    public void PostConfigure_NamedOptions_NotLogged()
    {
        var logger = new ListLogger();
        var postConfigure = new TelegramOptionsLogger(logger);

        postConfigure.PostConfigure(name: "named", new TelegramBotOptions { BotId = 42, BotSecret = "x" });

        logger.Messages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("http://[bad")] // битый URI
    [InlineData("proxy.local:8080")] // схема не указана — TryCreate парсит scheme="proxy.local"
    public void PostConfigure_MalformedProxyAddress_LoggedAsInvalidWithoutThrowing(string address)
    {
        var logger = new ListLogger();
        var options = new TelegramBotOptions
        {
            BotId = 42,
            BotSecret = "x",
            Proxy = new TelegramProxyOptions { Address = address },
        };
        var postConfigure = new TelegramOptionsLogger(logger);

        Should.NotThrow(() => postConfigure.PostConfigure(name: null, options));

        logger.Messages.ShouldHaveSingleItem().ShouldContain("некорректный");
    }

    private sealed class ListLogger : ILogger<TelegramOptionsLogger>
    {
        public List<string> Messages { get; } = [];

        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        bool ILogger.IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }
}
