using Bastilia.Rating.Domain;

namespace Bastilia.Rating.Portal.Common.Telegram;

public static class TelegramRegistration
{
    public static IServiceCollection AddTelegramNewsSender(this IServiceCollection services)
    {
        services.AddOptions<TelegramBotOptions>().BindConfiguration("Telegram");
        services.AddSingleton<ITelegramBotApi, TelegramBotApi>();
        services.AddTransient<ITelegramNewsSender, TelegramNewsSender>();
        return services;
    }
}
