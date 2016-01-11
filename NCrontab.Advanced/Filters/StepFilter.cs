using System;
using System.Collections.Generic;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Extensions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering ranges (i.e. 1-5)
    /// </summary>
    class StepFilter : ICronFilter
    {
        public CrontabFieldKind Kind { get; }
        public int Start { get; }
        public int Step { get; }

        /// <summary>
        /// Constructs a new RangeFilter instance
        /// </summary>
        /// <param name="start">The start of the range</param>
        /// <param name="end">The end of the range</param>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public StepFilter(int start, int step, CrontabFieldKind kind)
        {
            Start = start;
            Step = step;
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
            var evalValue = -1;
            switch (Kind)
            {
                case CrontabFieldKind.Second: evalValue = value.Second; break;
                case CrontabFieldKind.Minute: evalValue = value.Minute; break;
                case CrontabFieldKind.Hour: evalValue = value.Hour; break;
                case CrontabFieldKind.Day: evalValue = value.Day; break;
                case CrontabFieldKind.Month: evalValue = value.Month; break;
                case CrontabFieldKind.DayOfWeek: evalValue = value.DayOfWeek.ToCronDayOfWeek(); break;
                case CrontabFieldKind.Year: evalValue = value.Year; break;
                default: throw new ArgumentOutOfRangeException(nameof(Kind), Kind, null);
            }

            return IsMatch(evalValue);
        }

        private bool IsMatch(int evalValue)
        {
            return (evalValue - Start) % Step == 0;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Start == 0 ? "*" : Start.ToString(), Step);
        }

        public IEnumerable<SpecificFilter> ToSpecificFilters()
        {
            var maxValue = DateTimeExtensions.MaximumValues[Kind];
            for (var evalValue = Start; evalValue <= maxValue; evalValue++)
                if (IsMatch(evalValue))
                    yield return new SpecificFilter(evalValue, Kind);
        }

    }
}
