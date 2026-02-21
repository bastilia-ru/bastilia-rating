using Bastilia.Rating.Domain;
using Bastilia.Rating.Domain.DomainServices;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System.Text;

namespace Bastilia.Rating.Portal.AppServices
{
    public class ICalService(CalendarService calendarService)
    {
        public async Task<byte[]> GetCurrentIcalCalendar()
        {
            var currentYear = DateTime.UtcNow.Year;
            IReadOnlyCollection<BastiliaCalendarItem> items = [
                .. await calendarService.GetCalendarForYear(currentYear - 1),
            .. await calendarService.GetCalendarForYear(currentYear),
            .. await calendarService.GetCalendarForYear(currentYear + 1)
                ];

            var calendar = new Calendar();
            calendar.AddTimeZone(mskTimeZone);
            calendar.Events.AddRange(items.Select(i => BuildIcalEvent(i, mskTimeZone)));

            var serializer = new CalendarSerializer();
            return Encoding.UTF8.GetBytes(serializer.SerializeToString(calendar) ?? ""); //TODO stream
        }

        private CalendarEvent BuildIcalEvent(BastiliaCalendarItem i, TimeZoneInfo timeZone)
        {
            return new CalendarEvent
            {
                Start = new CalDateTime(i.StartDate, time: null, timeZone.Id),
                End = new CalDateTime(i.EndDate, time: null, timeZone.Id),
                Summary = i.Name,
                // TODO добавить ссылку
            };
        }

        private static readonly TimeZoneInfo mskTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
    }
}
