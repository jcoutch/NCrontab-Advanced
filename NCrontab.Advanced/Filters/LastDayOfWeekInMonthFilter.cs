using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Extensions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering for the last specified day of the week in the month
    /// </summary>
    public class LastDayOfWeekInMonthFilter : ICronFilter
    {
        public CrontabFieldKind Kind { get; }
        public int DayOfWeek { get; }
        private DayOfWeek DateTimeDayOfWeek { get; }

        /// <summary>
        /// Constructs a new instance of LastDayOfWeekInMonthFilter
        /// </summary>
        /// <param name="dayOfWeek">The cron day of the week (0 = Sunday...7 = Saturday)</param>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public LastDayOfWeekInMonthFilter(int dayOfWeek, CrontabFieldKind kind)
        {
            if (kind != CrontabFieldKind.DayOfWeek)
                throw new CrontabException(string.Format("<{0}L> can only be used in the Day of Week field.", dayOfWeek));

            DayOfWeek = dayOfWeek;
            DateTimeDayOfWeek = dayOfWeek.ToDayOfWeek();
            Kind = kind;
        }

        /// <summary>
        /// Checks if the value is accepted by the filter
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="kind">The kind of field being evaluated</param>
        /// <returns>True if the value matches the condition, False if it does not match.</returns>
        public bool IsMatch(DateTime value)
        {
            return value.Day == DateTimeDayOfWeek.LastDayOfMonth(value.Year, value.Month);
        }

        public override string ToString()
        {
            return string.Format("{0}L", DayOfWeek);
        }

    }
}
