using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles the filter instance where the user specifies a * (for any value)
    /// </summary>
    class AnyFilter : ICronFilter
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
    }
}
