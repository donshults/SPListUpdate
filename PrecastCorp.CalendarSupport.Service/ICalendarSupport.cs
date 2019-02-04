using PrecastCorp.CalendarSupport.Service.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrecastCorp.CalendarSupport.Service
{
    public interface ICalendarSupport
    {
        IEnumerable<CalendarItem> GetCalendarItems(string siteUrl, string listName, string timeZoneId);
        CalendarItem GetCalendarItemById(string siteUrl, string listName, int itemId);
        string GetCalendarTitle();
        bool DeleteCalendarItemById(string siteUrl, string listName, int itemId);
        CalendarItem AddCalendarItem(CalendarItem calItem);
        CalendarItem UpdateCalendarItem(CalendarItem calItem);
        IReadOnlyCollection<TimeZoneInfo> GetTimeZoneInfo();
        Task<string> GetAccessTokenAsync();

    }
}