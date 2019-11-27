using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Extensions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering for a specific value
    /// </summary>
    public class SpecificYearFilter : SpecificFilter
    {
        /// <summary>
        /// Constructs a new RangeFilter instance
        /// </summary>
        /// <param name="specificValue">The specific value you wish to match</param>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public SpecificYearFilter(int specificValue, CrontabFieldKind kind) : base (specificValue, kind) {}

        public override int? Next(int value)
        {
            if (value < SpecificValue)
                return SpecificValue;

            return null;
        }

        public override int? Previous(int value)
        {
            if (value > SpecificValue)
                return SpecificValue;

            return null;
        }
    }
}
