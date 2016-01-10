using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCrontab.Advanced.Extensions
{
    internal static class DayOfWeekExtensions
    {
        /// <summary>
        /// Since there is no guarantee that (int) DayOfWeek returns the same value
        /// that cron uses (since the values aren't explicitly set in the DayOfWeek enum)
        /// we have to use this method.
        /// </summary>
        /// <param name="value">The DayOfWeek value to convert</param>
        /// <returns>An integer representing the provided day of week</returns>
        internal static int ToCronDayOfWeek(this DayOfWeek value)
        {
            switch (value)
            {
                case DayOfWeek.Sunday: return 0;
                case DayOfWeek.Monday: return 1;
                case DayOfWeek.Tuesday: return 2;
                case DayOfWeek.Wednesday: return 3;
                case DayOfWeek.Thursday: return 4;
                case DayOfWeek.Friday: return 5;
                case DayOfWeek.Saturday: return 6;
                default: throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        internal static DayOfWeek ToDayOfWeek(this int value)
        {
            switch (value)
            {
                case 0: return DayOfWeek.Sunday;
                case 1: return DayOfWeek.Monday;
                case 2: return DayOfWeek.Tuesday;
                case 3: return DayOfWeek.Wednesday;
                case 4: return DayOfWeek.Thursday;
                case 5: return DayOfWeek.Friday;
                case 6: return DayOfWeek.Saturday;
                default: throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        internal static int LastDayOfMonth(this DayOfWeek dayOfWeek, int year, int month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var date = new DateTime(year, month, daysInMonth);
            while (date.DayOfWeek != dayOfWeek)
                date = date.AddDays(-1);

            return date.Day;
        }
    }
}
