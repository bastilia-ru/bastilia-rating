namespace Bastilia.Rating.Database;

internal class BastiliaKograIgraRepository(AppDbContext context) : IBastiliaKograIgraRepository
{
    public async Task<IReadOnlyCollection<BastiliaCalendarItem>> GetGameCalendarFor(int year)
    {
        return await context.KogdaIgraGames
            .Where(ki => ki.StartDate.Year == year || ki.EndDate.Year == year)
            .Select(ki => new BastiliaCalendarItem(BastiliaCalendarItemType.Game, ki.StartDate, ki.EndDate, ki.Name, ki.KogdaIgraGameId))
            .ToListAsync();
    }
}
