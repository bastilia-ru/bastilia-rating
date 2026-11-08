namespace Bastilia.Rating.Database.DbServices
{
    internal class UserDbService(IDbContextFactory<AppDbContext> contextFactory) : IUserDbService
    {
        public async Task<BastiliaMember> AddUser(int playerId, string nickName, string avatarUrl)
        {
            var user = new Entities.User()
            {
                AvatarUrl = avatarUrl,
                Username = nickName,
                JoinRpgUserId = playerId,
                ParticipateInRating = true,
            };

            await using (var appDbContext = await contextFactory.CreateDbContextAsync())
            {
                await appDbContext.Set<Entities.User>().AddAsync(user);
                await appDbContext.SaveChangesAsync();
            }

            var rep = new BastiliaMemberRepository(contextFactory);
            return await rep.GetByIdAsync(playerId) ?? throw new InvalidOperationException();
        }

        public async Task<BastiliaMember> UpdateUser(int playerId, string? nickName, string? avatarUrl)
        {
            await using (var appDbContext = await contextFactory.CreateDbContextAsync())
            {
                var user = await appDbContext.Set<Entities.User>().SingleAsync(u => u.JoinRpgUserId == playerId);
                if (nickName is not null)
                {
                    user.Username = nickName;
                }
                if (avatarUrl is not null)
                {
                    user.AvatarUrl = avatarUrl;
                }
                await appDbContext.SaveChangesAsync();
            }

            var rep = new BastiliaMemberRepository(contextFactory);
            return await rep.GetByIdAsync(playerId) ?? throw new InvalidOperationException();
        }
    }
}
