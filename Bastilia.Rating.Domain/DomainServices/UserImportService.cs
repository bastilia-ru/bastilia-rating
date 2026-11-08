using JoinRpg.Client;
using Microsoft.Extensions.Logging;

namespace Bastilia.Rating.Domain.DomainServices;

public class UserImportService(IBastiliaMemberRepository bastiliaMemberRepository, JoinUserInfoClient joinUserInfoClient, IUserDbService userDbService,
    ILogger<UserImportService> logger)
{
    public async Task<BastiliaMember?> ImportUser(int userId)
    {
        var user = await bastiliaMemberRepository.GetByIdAsync(userId);
        if (user is not null)
        {
            return user;
        }

        var info = await joinUserInfoClient.GetUserInfo(userId);
        if (info is not null)
        {
            return await userDbService.AddUser(info.PlayerId, info.NickName, info.AvatarUrl);
        }

        logger.LogWarning("Не удалось загрузить пользователя {userId}", userId);
        return null;
    }

    public async Task<BastiliaMember?> ImportUser(int userId, string? nickName, string? avatarUrl)
    {
        var user = await bastiliaMemberRepository.GetByIdAsync(userId);
        if (user is not null)
        {
            if ((nickName is not null && nickName != user.UserName) || (avatarUrl is not null && avatarUrl != user.AvatarUrl))
            {
                return await userDbService.UpdateUser(userId, nickName, avatarUrl);
            }
            return user;
        }

        if (nickName is not null)
        {
            return await userDbService.AddUser(userId, nickName, avatarUrl ?? "");
        }

        return await ImportUser(userId);
    }
}
