using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles the filter instance where the user specifies a * (for any value)
    /// </summary>
    public class AnyFilter : ICronFilter, ITimeFilter
    {
        public CrontabFieldKind Kind { get; }

        /// <summary>
        /// Constructs a new AnyFilter instance
        /// </summary>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public AnyFilter(CrontabFieldKind kind)
        {
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
            return true;
        }

        public int? Next(int value)
        {
            var max = Constants.MaximumDateTimeValues[Kind];
            if (Kind == CrontabFieldKind.Day
             || Kind == CrontabFieldKind.Month
             || Kind == CrontabFieldKind.DayOfWeek)
                throw new CrontabException("Cannot call Next for Day, Month or DayOfWeek types");

            var newValue = (int?) value + 1;
            if (newValue > max) newValue = null;

            return newValue;
        }

        public int? Previous(int value)
        {
            var min = Constants.MinimumDateTimeValues[Kind];
            if (Kind == CrontabFieldKind.Day
                || Kind == CrontabFieldKind.Month
                || Kind == CrontabFieldKind.DayOfWeek)
                throw new CrontabException("Cannot call Next for Day, Month or DayOfWeek types");

            var newValue = (int?)value - 1;
            if (newValue < min) newValue = null;

            return newValue;
        }

        public int First()
        {
            if (Kind == CrontabFieldKind.Day
             || Kind == CrontabFieldKind.Month
             || Kind == CrontabFieldKind.DayOfWeek)
                throw new CrontabException("Cannot call First for Day, Month or DayOfWeek types");

            return 0;
        }

        public int Last()
        {
            if (Kind == CrontabFieldKind.Day
                || Kind == CrontabFieldKind.Month
                || Kind == CrontabFieldKind.DayOfWeek)
                throw new CrontabException("Cannot call Last for Day, Month or DayOfWeek types");

            return Constants.MaximumDateTimeValues[Kind];
        }

        public override string ToString()
        {
            return "*";
        }
    }
}
