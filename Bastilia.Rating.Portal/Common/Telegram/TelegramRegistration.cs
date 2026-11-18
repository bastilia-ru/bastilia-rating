using Bastilia.Rating.Domain;
using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Common.Telegram;

public static class TelegramRegistration
{
    public static IServiceCollection AddTelegramNewsSender(this IServiceCollection services)
    {
        services.AddOptions<TelegramBotOptions>().BindConfiguration("Telegram");
        // Логируем эффективную конфигурацию (прокси!, токен/пароль маскируем) —
        // иначе «приложение не видит прокси» неотличимо от «прокси не отвечает».
        services.AddSingleton<IPostConfigureOptions<TelegramBotOptions>, TelegramOptionsLogger>();
        // Десериализация Update из тела webhook (snake_case) средствами Telegram.Bot.
        services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);
        services.AddSingleton<TelegramBotApi>();
        services.AddSingleton<ITelegramBotApi>(sp => new LoggingTelegramBotApi(
            sp.GetRequiredService<TelegramBotApi>(),
            sp.GetRequiredService<ILogger<LoggingTelegramBotApi>>()));
        services.AddTransient<ITelegramNewsSender, TelegramNewsSender>();
        services.AddTransient<NewsMessageBuilder>();
        services.AddTransient<NewsHtmlRenderer>();
        services.AddTransient<IAchievementGrantedNotifier, AchievementGrantedNotifier>();
        services.AddTransient<AchievementMessageBuilder>();
        services.AddTransient<WelcomeMessageBuilder>();
        services.AddTransient<TelegramUpdateHandler>();
        services.AddHostedService<TelegramWebhookRegistrar>();
        services.AddHostedService<TelegramLongPollingService>();

        // TODO[JoinRpg.Common.WebInfrastructure]: meter'ы System.Net.Http / System.Net.NameResolution
        // продублированы в AddJoinOpenTelemetry (joinrpg-net) — выпилить этот вызов при обновлении пакета.
        services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddMeter("System.Net.Http", "System.Net.NameResolution"));

        // Telegram — опциональная интеграция, поэтому без тега "ready": выключенный
        // Telegram не должен задерживать /health/ready.
        services.AddHealthChecks().AddCheck<TelegramHealthCheck>("Telegram client");

        return services;
    }
}
