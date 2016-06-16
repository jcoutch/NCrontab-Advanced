using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Extensions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering for a specific day of the week in the month (i.e. 3rd Tuesday of the month)
    /// </summary>
    public class SpecificDayOfWeekInMonthFilter : ICronFilter
    {
        public CrontabFieldKind Kind { get; }
        public int DayOfWeek { get; }
        public int WeekNumber { get; }
        private DayOfWeek DateTimeDayOfWeek { get; }

        /// <summary>
        /// Constructs a new instance of LastDayOfWeekInMonthFilter
        /// </summary>
        /// <param name="dayOfWeek">The cron day of the week (0 = Sunday...7 = Saturday)</param>
        /// <param name="weekNumber">Indicates which occurence of the day to filter against</param>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public SpecificDayOfWeekInMonthFilter(int dayOfWeek, int weekNumber, CrontabFieldKind kind)
        {
            if (weekNumber <= 0 || weekNumber > 5)
                throw new CrontabException($"Week number = {0} is out of bounds.");

            if (kind != CrontabFieldKind.DayOfWeek)
                throw new CrontabException($"<{dayOfWeek}#{weekNumber}> can only be used in the Day of Week field.");

            DayOfWeek = dayOfWeek;
            DateTimeDayOfWeek = dayOfWeek.ToDayOfWeek();
            WeekNumber = weekNumber;
            Kind = kind;
        }

        /// <summary>
        /// Checks if the value is accepted by the filter
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value matches the condition, False if it does not match.</returns>
        public bool IsMatch(DateTime value)
        {
            var weekCount = 0;
            var currentDay = new DateTime(value.Year, value.Month, 1);
            while (currentDay.Month == value.Month)
            {
                if (currentDay.DayOfWeek == DateTimeDayOfWeek)
                {
                    weekCount++;
                    if (weekCount == WeekNumber) break;
                    currentDay = currentDay.AddDays(7);
                }
                else
                {
                    currentDay = currentDay.AddDays(1);
                }
            }

            if (currentDay.Month != value.Month) return false;

            return value.Day == currentDay.Day;
        }

        public override string ToString()
        {
            return $"{DayOfWeek}#{WeekNumber}";            
        }

    }
}
