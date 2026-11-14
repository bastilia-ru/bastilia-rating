using JoinRpg.Client;
using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Logging;

namespace Bastilia.Rating.Domain.DomainServices;

public class UserImportService(IBastiliaMemberRepository bastiliaMemberRepository, JoinUserInfoClient joinUserInfoClient, IUserDbService userDbService,
    ILogger<UserImportService> logger)
{
    public Task<BastiliaMember?> ImportUser(int userId) => ImportUserById(userId);

    public async Task<BastiliaMember?> ImportUserById(int userId, TelegramSocialLink? telegram = null)
    {
        var user = await bastiliaMemberRepository.GetByIdAsync(userId);
        if (user is not null)
        {
            return await UpdateTelegramIfChanged(user, telegram) ?? user;
        }

        logger.LogInformation("Пользователь {UserId} не найден в локальной БД при логине; запрашиваем данные из JoinRPG", userId);
        var info = await joinUserInfoClient.GetUserInfo(userId);
        if (info is not null)
        {
            return await userDbService.AddUser(info.PlayerId, info.NickName, info.AvatarUrl, telegram);
        }

        logger.LogWarning("Не удалось загрузить пользователя {userId}", userId);
        return null;
    }

    public async Task<BastiliaMember?> ImportUser(int userId, string? nickName, string? avatarUrl, TelegramSocialLink? telegram = null)
    {
        var user = await bastiliaMemberRepository.GetByIdAsync(userId);
        if (user is not null)
        {
            var profileChanged = (nickName is not null && nickName != user.UserName) || (avatarUrl is not null && avatarUrl != user.AvatarUrl);
            var telegramChanged = IsTelegramChanged(user, telegram);
            if (profileChanged || telegramChanged)
            {
                if (profileChanged)
                {
                    logger.LogInformation("При логине изменились данные пользователя {UserId}: ник '{OldNickName}' → '{NewNickName}', аватар '{OldAvatarUrl}' → '{NewAvatarUrl}'",
                        userId, user.UserName, nickName, user.AvatarUrl, avatarUrl);
                }
                return await userDbService.UpdateUser(userId, nickName, avatarUrl, telegramChanged ? telegram : null);
            }
            return user;
        }

        if (nickName is not null)
        {
            logger.LogInformation("Пользователь {UserId} не найден в локальной БД при логине; создаём из OIDC claims (ник '{NickName}')", userId, nickName);
            return await userDbService.AddUser(userId, nickName, avatarUrl ?? "", telegram);
        }

        return await ImportUserById(userId, telegram);
    }

    private bool IsTelegramChanged(BastiliaMember user, TelegramSocialLink? telegram)
    {
        if (telegram is null)
        {
            // Сборка ссылки — в вызывающем коде: TelegramSocialLink без chatId не строится,
            // поэтому null-ссылка означает, что claim telegram_chat_id отсутствовал, и привязку
            // обновлять не нужно.
            if (user.Telegram is not null)
            {
                logger.LogInformation("В IdPortal нет claims telegram/telegram_chat_id для пользователя {UserId}, хотя в БД есть привязка {Telegram}; существующая привязка сохранена",
                    user.JoinrpgUserId, user.Telegram);
            }
            return false;
        }

        if (telegram.Id != user.Telegram?.Id)
        {
            return true;
        }
        // Username обновляем, только если claim пришёл и отличается; отсутствие claim не затирает имя в БД.
        return telegram.PrettyName is not null && telegram.PrettyName.Value != user.Telegram?.PrettyName?.Value;
    }

    private async Task<BastiliaMember?> UpdateTelegramIfChanged(BastiliaMember user, TelegramSocialLink? telegram)
    {
        if (!IsTelegramChanged(user, telegram))
        {
            return null;
        }

        logger.LogInformation("При логине изменилась привязка Telegram пользователя {UserId}: '{OldTelegram}' → '{NewTelegram}'",
            user.JoinrpgUserId, user.Telegram, telegram);
        return await userDbService.UpdateUser(user.JoinrpgUserId, null, null, telegram);
    }
}
