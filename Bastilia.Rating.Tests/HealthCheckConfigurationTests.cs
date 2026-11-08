using Bastilia.Rating.Portal.Common;
using Bastilia.Rating.Portal.Common.Telegram;
using JoinRpg.Common.WebInfrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Tests;

/// <summary>
/// Регрессия на настройку health-чеков: чек без явного timeout: получает дефолтный таймаут
/// (иначе вместе с ним висит весь /health), а чек базы данных участвует в /health/ready.
/// </summary>
public class HealthCheckConfigurationTests
{
    [Fact]
    public void CheckWithoutTimeoutGetsDefaultOne()
    {
        var registrations = BuildRegistrations(checks => checks
            .AddCheck("no-timeout", () => HealthCheckResult.Healthy()));

        registrations["no-timeout"].Timeout.ShouldBe(HealthCheckExtensions.DefaultHealthCheckTimeout);
    }

    [Fact]
    public void DatabaseCheckGetsReadyTag()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks().AddCheck("BastiliaRating-db", () => HealthCheckResult.Healthy());
        services.AddDatabaseHealthCheck(CreateConfiguration(withConnectionString: true));

        var registration = ResolveRegistrations(services)["BastiliaRating-db"];

        registration.Tags.ShouldContain("ready");
    }

    [Fact]
    public void DatabaseCheckWithoutConnectionString_DoesNothing()
    {
        var services = new ServiceCollection();
        services.AddDatabaseHealthCheck(CreateConfiguration(withConnectionString: false));

        services.Any(descriptor => descriptor.ServiceType == typeof(IConfigureOptions<HealthCheckServiceOptions>))
            .ShouldBeFalse();
    }

    [Fact]
    public void NullProxySectionBindsToNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Telegram:BotId"] = "42",
                ["Telegram:BotSecret"] = "secret",
                ["Telegram:Proxy"] = null,
            })
            .Build();

        var options = BindTelegramOptions(configuration);

        options.Proxy.ShouldBeNull();
    }

    [Fact]
    public void ProxySectionBindsToOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Telegram:BotId"] = "42",
                ["Telegram:BotSecret"] = "secret",
                ["Telegram:Proxy:Address"] = "socks5://proxy:1080",
                ["Telegram:Proxy:Username"] = "user",
                ["Telegram:Proxy:Password"] = "pass",
            })
            .Build();

        var options = BindTelegramOptions(configuration);

        options.Proxy.ShouldNotBeNull();
        options.Proxy.Address.ShouldBe("socks5://proxy:1080");
        options.Proxy.Username.ShouldBe("user");
        options.Proxy.Password.ShouldBe("pass");
    }

    private static Dictionary<string, HealthCheckRegistration> BuildRegistrations(
        Action<IHealthChecksBuilder> registerChecks)
    {
        var services = new ServiceCollection();
        registerChecks(services.AddHealthChecks());
        services.AddJoinHealthChecks();

        return ResolveRegistrations(services);
    }

    private static Dictionary<string, HealthCheckRegistration> ResolveRegistrations(IServiceCollection services)
        => services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations
            .ToDictionary(registration => registration.Name);

    private static IConfiguration CreateConfiguration(bool withConnectionString)
    {
        var values = new Dictionary<string, string?>();
        if (withConnectionString)
        {
            values["ConnectionStrings:BastiliaRating"] = "Host=localhost";
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static TelegramBotOptions BindTelegramOptions(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddOptions<TelegramBotOptions>().BindConfiguration("Telegram");
        return services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<TelegramBotOptions>>()
            .Value;
    }
}
