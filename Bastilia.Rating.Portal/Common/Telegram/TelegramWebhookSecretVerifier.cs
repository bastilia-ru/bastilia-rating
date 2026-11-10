using System.Security.Cryptography;
using System.Text;

namespace Bastilia.Rating.Portal.Common.Telegram;

internal static class TelegramWebhookSecretVerifier
{
    public static bool IsValid(string? configuredSecret, string? providedSecret)
    {
        if (string.IsNullOrEmpty(configuredSecret) || string.IsNullOrEmpty(providedSecret))
        {
            return false;
        }

        var configuredBytes = Encoding.UTF8.GetBytes(configuredSecret);
        var providedBytes = Encoding.UTF8.GetBytes(providedSecret);
        return configuredBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(configuredBytes, providedBytes);
    }
}
