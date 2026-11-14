using JoinRpg.Common.PrimitiveTypes;
using Microsoft.Extensions.Logging;

namespace Bastilia.Rating.Database.DbServices
{
    internal class UserDbService(IDbContextFactory<AppDbContext> contextFactory, ILogger<UserDbService> logger) : IUserDbService
    {
        public async Task<BastiliaMember> AddUser(int playerId, string nickName, string avatarUrl, TelegramSocialLink? telegram = null)
        {
            logger.LogInformation("Создаём нового пользователя {JoinRpgUserId} с ником '{Username}', Telegram '{Telegram}'",
                playerId, nickName, telegram);
            var user = new Entities.User()
            {
                AvatarUrl = avatarUrl,
                Username = nickName,
                JoinRpgUserId = playerId,
                ParticipateInRating = true,
                TelegramChatId = telegram?.Id,
                TelegramUsername = telegram?.PrettyName?.Value,
            };

            await using (var appDbContext = await contextFactory.CreateDbContextAsync())
            {
                await appDbContext.Set<Entities.User>().AddAsync(user);
                await appDbContext.SaveChangesAsync();
            }

            var rep = new BastiliaMemberRepository(contextFactory);
            return await rep.GetByIdAsync(playerId) ?? throw new InvalidOperationException();
        }

        public async Task<BastiliaMember> UpdateUser(int playerId, string? nickName, string? avatarUrl, TelegramSocialLink? telegram = null)
        {
            await using (var appDbContext = await contextFactory.CreateDbContextAsync())
            {
                var user = await appDbContext.Set<Entities.User>().SingleAsync(u => u.JoinRpgUserId == playerId);
                var oldUsername = user.Username;
                var oldAvatarUrl = user.AvatarUrl;
                var oldTelegramChatId = user.TelegramChatId;
                var oldTelegramUsername = user.TelegramUsername;
                if (nickName is not null)
                {
                    user.Username = nickName;
                }
                if (avatarUrl is not null)
                {
                    user.AvatarUrl = avatarUrl;
                }
                if (telegram?.Id is not null)
                {
                    user.TelegramChatId = telegram.Id;
                }
                if (telegram?.PrettyName is not null)
                {
                    user.TelegramUsername = telegram.PrettyName.Value;
                }
                await appDbContext.SaveChangesAsync();
                logger.LogInformation(
                    "Обновлён пользователь {JoinRpgUserId}: ник '{OldUsername}' → '{NewUsername}', аватар '{OldAvatarUrl}' → '{NewAvatarUrl}', TelegramChatId '{OldTelegramChatId}' → '{NewTelegramChatId}', имя Telegram '@{OldTelegramUsername}' → '@{NewTelegramUsername}'",
                    playerId, oldUsername, user.Username, oldAvatarUrl, user.AvatarUrl, oldTelegramChatId, user.TelegramChatId, oldTelegramUsername, user.TelegramUsername);
            }

            var rep = new BastiliaMemberRepository(contextFactory);
            return await rep.GetByIdAsync(playerId) ?? throw new InvalidOperationException();
        }
    }
}
