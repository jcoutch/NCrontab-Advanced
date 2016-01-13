using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering for the last day of the month
    /// </summary>
    public class LastDayOfMonthFilter : ICronFilter
    {
        public CrontabFieldKind Kind { get; }

        public LastDayOfMonthFilter(CrontabFieldKind kind)
        {
            if (kind != CrontabFieldKind.Day)
                throw new CrontabException("The <L> filter can only be used with the Day field.");

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
            return DateTime.DaysInMonth(value.Year, value.Month) == value.Day;
        }
        public override string ToString()
        {
            return "L";
        }
    }
}
