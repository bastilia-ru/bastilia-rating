


namespace Bastilia.Rating.Domain.DomainServices;

public class CalendarService(IBastiliaMemberRepository bastiliaMemberRepository, IBastiliaProjectRepository bastiliaProjectRepository)
{
    public async Task<IReadOnlyCollection<BastiliaCalendarItem>> GetCalendarForYear(int year)
    {
        var memberQuery = await bastiliaMemberRepository.GetMemberCalendarFor(year);
        var projectEvents = await bastiliaProjectRepository.GetProjectCalendarFor(year);

        var memberEvents = RemoveNotRequired(LinkRelatedEvents(memberQuery));
        return [.. memberEvents.Union(projectEvents)];
    }

    private IEnumerable<BastiliaCalendarItem> RemoveNotRequired(IReadOnlyCollection<BastiliaCalendarItem> bastiliaCalendarItems)
    {
        return bastiliaCalendarItems
            .Where(i => !(i.Type == BastiliaCalendarItemType.Birthday && i.LinkedItem is not null && i.LinkedItem.StartDate <= i.StartDate && i.LinkedItem.EndDate >= i.EndDate));
    }

    internal IReadOnlyCollection<BastiliaCalendarItem> LinkRelatedEvents(IReadOnlyCollection<BastiliaCalendarItem> bastiliaCalendarItems)
    {
        var list = bastiliaCalendarItems.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            // Тут есть проблема в том, что LinkedItem из старой коллекции, и по LinkedItem нельзя шагать далее. Возможно это не страшно.
            if (TryFindRelatedItem(item, bastiliaCalendarItems) is BastiliaCalendarItem relatedItem)
            {
                list[i] = item with { LinkedItem = relatedItem };
            }
        }
        return list;
    }

    private BastiliaCalendarItem? TryFindRelatedItem(BastiliaCalendarItem item, IReadOnlyCollection<BastiliaCalendarItem> bastiliaCalendarItems)
    {
        if (item.Type == BastiliaCalendarItemType.Birthday)
        {
            if (bastiliaCalendarItems.FirstOrDefault(i => i.Id == item.Id && i.Type == BastiliaCalendarItemType.BirthdayParty) is BastiliaCalendarItem related)
            {
                return related;
            }
        }


        if (item.Type == BastiliaCalendarItemType.BirthdayParty)
        {
            if (bastiliaCalendarItems.FirstOrDefault(i => i.Id == item.Id && i.Type == BastiliaCalendarItemType.Birthday) is BastiliaCalendarItem related)
            {
                return related;
            }
        }

        return null;
    }
}
