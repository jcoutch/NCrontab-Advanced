using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            return Constants.CronDays[value];
        }

        /// <summary>
        /// Since there is no guarantee that (int) DayOfWeek returns the same value
        /// that cron uses (since the values aren't explicitly set in the DayOfWeek enum)
        /// we have to use this method.
        /// </summary>
        /// <param name="value">The cron day value to convert</param>
        /// <returns>A DayOfWeek representing the provided day of week</returns>
        internal static DayOfWeek ToDayOfWeek(this int value)
        {
            return Constants.CronDays.First(x => x.Value == value).Key;
        }

        /// <summary>
        /// Retrieves the last instance of the specified day of the month
        /// </summary>
        /// <param name="dayOfWeek">The day you want to find</param>
        /// <param name="year">The year in which you want to find the day</param>
        /// <param name="month">The month in which you want to find the day</param>
        /// <returns>An integer representing the day that matches the criteria</returns>
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
