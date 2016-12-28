using System;
using System.Reflection;

namespace HomeAppsLib.TypeExtensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToEndOfDay(this DateTime dt)
        {
            return dt.AddDays(1).Date.AddMilliseconds(-1);
        }
        public static string ToLongDateDisplay(this DateTime dt)
        {
            return dt.DayOfWeek + " " + dt.ToShortDateString();
        }
        public static string ToLongDateTimeDisplay(this DateTime dt)
        {
            return dt.DayOfWeek + " " + dt.ToShortDateString() + " at " + dt.ToShortTimeString();
        }
    }
}
