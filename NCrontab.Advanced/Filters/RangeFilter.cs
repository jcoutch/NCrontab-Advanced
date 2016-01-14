using System;
using System.Collections.Generic;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Extensions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering ranges (i.e. 1-5)
    /// </summary>
    public class RangeFilter : ICronFilter, ITimeFilter
    {
        public CrontabFieldKind Kind { get; }
        public int Start { get; }
        public int End { get; }
        public int? Steps { get; }

        private int? FirstCache { get; set; }

        /// <summary>
        /// Returns a list of specific filters that represents this step filter.
        /// NOTE - This is only populated on construction, and should NOT be modified.
        /// </summary>
        public IEnumerable<SpecificFilter> SpecificFilters { get; }

        /// <summary>
        /// Constructs a new RangeFilter instance
        /// </summary>
        /// <param name="start">The start of the range</param>
        /// <param name="end">The end of the range</param>
        /// <param name="steps">The steps in the range</param>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public RangeFilter(int start, int end, int? steps, CrontabFieldKind kind)
        {
            var maxValue = Constants.MaximumDateTimeValues[kind];

            if (start < 0 || start > maxValue)
                throw new CrontabException(string.Format("Start = {0} is out of bounds for <{1}> field", start, Enum.GetName(typeof(CrontabFieldKind), kind)));

            if (end < 0 || end > maxValue)
                throw new CrontabException(string.Format("End = {0} is out of bounds for <{1}> field", end, Enum.GetName(typeof(CrontabFieldKind), kind)));

            if (steps != null && (steps <= 0 || steps > maxValue))
                throw new CrontabException(string.Format("Steps = {0} is out of bounds for <{1}> field", steps, Enum.GetName(typeof(CrontabFieldKind), kind)));

            Start = start;
            End = end;
            Kind = kind;
            Steps = steps;

            var filters = new List<SpecificFilter>();
            for (var evalValue = Start; evalValue <= End; evalValue++)
                if (IsMatch(evalValue))
                    filters.Add(new SpecificFilter(evalValue, Kind));

            SpecificFilters = filters;
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
            return evalValue >= Start && evalValue <= End && (!Steps.HasValue || ((evalValue - Start) % Steps) == 0);
        }

        public int? Next(int value)
        {
            if (Kind == CrontabFieldKind.Day
             || Kind == CrontabFieldKind.Month
             || Kind == CrontabFieldKind.DayOfWeek)
                throw new CrontabException("Cannot call Next for Day, Month or DayOfWeek types");

            var max = Constants.MaximumDateTimeValues[Kind];

            var newValue = (int?) value + 1;
            while (newValue < max && !IsMatch(newValue.Value))
                newValue++;

            if (newValue >= max) newValue = null;

            return newValue;
        }

        public int First()
        {
            if (FirstCache.HasValue) return FirstCache.Value;

            if (Kind == CrontabFieldKind.Day
             || Kind == CrontabFieldKind.Month
             || Kind == CrontabFieldKind.DayOfWeek)
                throw new CrontabException("Cannot call First for Day, Month or DayOfWeek types");

            var max = Constants.MaximumDateTimeValues[Kind];

            var newValue = 0;
            while (newValue < max && !IsMatch(newValue))
                newValue++;

            if (newValue > max)
                throw new CrontabException(string.Format("Next value for {0} on field {1} could not be found!",
                    this.ToString(),
                    Enum.GetName(typeof(CrontabFieldKind), Kind))
                );

            FirstCache = newValue;
            return newValue;
        }

        public override string ToString()
        {
            if (Steps.HasValue)
                return string.Format("{0}-{1}/{2}", Start, End, Steps);
            else
                return string.Format("{0}-{1}", Start, End);
        }
    }
}
