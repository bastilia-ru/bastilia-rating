using Microsoft.Extensions.Options;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Логирует эффективную конфигурацию Telegram при первом обращении к опциям:
/// видно, что реально увидело приложение, а не что лежит в секрете кластера.
/// Секреты (BotSecret, пароль прокси) не логируются — только их наличие.
/// Никогда не бросает исключений: кривой адрес прокси — это диагноз в логе,
/// а не повод ронять хост на старте.
/// </summary>
internal class TelegramOptionsLogger(ILogger<TelegramOptionsLogger> logger) : IPostConfigureOptions<TelegramBotOptions>
{
    private static readonly string[] KnownProxySchemes = ["http", "https", "socks4", "socks4a", "socks5", "socks5h"];

    public void PostConfigure(string? name, TelegramBotOptions options)
    {
        // Безымянные опции кешируются на весь lifetime, так что лог будет один.
        // Именованные (если вдруг появятся) пропускаем, чтобы не плодить строки.
        if (!string.IsNullOrEmpty(name))
        {
            return;
        }

        logger.LogInformation(
            "Telegram сконфигурирован: enabled={Enabled}, botId={BotId}, chatId={ChatId}, transport={Transport}, proxy={Proxy}",
            options.Enabled,
            options.BotId,
            options.ChatId,
            options.Transport,
            FormatProxy(options.Proxy));
    }

    private static string FormatProxy(TelegramProxyOptions? proxy)
    {
        if (proxy is null)
        {
            return "не задан (прямое подключение)";
        }

        // Адрес без схемы ("proxy.local:8080") Uri.TryCreate парсит как scheme="proxy.local" —
        // обязательно проверяем схему по белому списку.
        if (!Uri.TryCreate(proxy.Address, UriKind.Absolute, out var uri)
            || Array.IndexOf(KnownProxySchemes, uri.Scheme) < 0)
        {
            return $"задан, но адрес некорректный: '{proxy.Address}' (ожидается http://, https:// или socks5://host:port)";
        }

        return $"{uri.Scheme}://{uri.Host}:{uri.Port}, user={(string.IsNullOrEmpty(proxy.Username) ? "не задан" : "задан")}, password={(string.IsNullOrEmpty(proxy.Password) ? "не задан" : "задан")}";
    }
}
