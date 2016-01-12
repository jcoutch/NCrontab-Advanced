using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering for the last weekday of a month
    /// </summary>
    class LastWeekdayOfMonthFilter : ICronFilter
    {
        public CrontabFieldKind Kind { get; }

        /// <summary>
        /// Constructs a new RangeFilter instance
        /// </summary>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public LastWeekdayOfMonthFilter(CrontabFieldKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Checks if the value is accepted by the filter
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value matches the condition, False if it does not match.</returns>
        public bool IsMatch(DateTime value)
        {
            var specificValue = DateTime.DaysInMonth(value.Year, value.Month);
            var specificDay = new DateTime(value.Year, value.Month, specificValue);

            DateTime closestWeekday;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (specificDay.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    // If the specified day is Saturday, back up to Friday
                    closestWeekday = specificDay.AddDays(-1);

                    // If Friday is in the previous month, then move forward to the following Monday
                    if (closestWeekday.Month != specificDay.Month)
                        closestWeekday = specificDay.AddDays(2);

                    break;

                case DayOfWeek.Sunday:
                    // If the specified day is Sunday, move forward to Monday
                    closestWeekday = specificDay.AddDays(1);

                    // If Monday is in the next month, then move backward to the previous Friday
                    if (closestWeekday.Month != specificDay.Month)
                        closestWeekday = specificDay.AddDays(-2);

                    break;

                default:
                    // The specified day happens to be a weekday, so use it
                    closestWeekday = specificDay;
                    break;
            }

            return value.Day == closestWeekday.Day;
        }

        public override string ToString()
        {
            return "LW";
        }
    }
}
