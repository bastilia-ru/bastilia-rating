using Bastilia.Rating.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bastilia.Rating.Portal.Common.Telegram;

public static class TelegramRegistration
{
    public static IServiceCollection AddTelegramNewsSender(this IServiceCollection services)
    {
        services.AddOptions<TelegramBotOptions>().BindConfiguration("Telegram");
        // Десериализация Update из тела webhook (snake_case) средствами Telegram.Bot.
        services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);
        services.AddSingleton<ITelegramBotApi, TelegramBotApi>();
        services.AddTransient<ITelegramNewsSender, TelegramNewsSender>();
        services.AddTransient<NewsMessageBuilder>();
        services.AddTransient<NewsHtmlRenderer>();
        services.AddTransient<IAchievementGrantedNotifier, AchievementGrantedNotifier>();
        services.AddTransient<AchievementMessageBuilder>();
        services.AddTransient<WelcomeMessageBuilder>();
        services.AddTransient<TelegramUpdateHandler>();
        services.AddHostedService<TelegramWebhookRegistrar>();

        // Telegram — опциональная интеграция, поэтому без тега "ready": выключенный
        // Telegram не должен задерживать /health/ready.
        services.AddHealthChecks().AddCheck<TelegramHealthCheck>("Telegram client");

        return services;
    }
}
