namespace Bastilia.Rating.Database.DbServices
{
    internal class UserDbService(AppDbContext appDbContext) : IUserDbService
    {
        public async Task<BastiliaMember> AddUser(int playerId, string nickName, string avatarUrl)
        {
            var user = new Entities.User()
            {
                AvatarUrl = avatarUrl,
                Username = nickName,
                JoinRpgUserId = playerId
            };
            await appDbContext.Set<Entities.User>().AddAsync(user);
            await appDbContext.SaveChangesAsync();

            var rep = new BastiliaMemberRepository(appDbContext);
            return await rep.GetByIdAsync(playerId) ?? throw new InvalidOperationException();
        }
    }
}
