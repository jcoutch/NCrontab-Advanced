using System;
using NCrontab.Advanced.Enumerations;

namespace NCrontab.Advanced.Interfaces
{
    public interface ICronFilter
    {
        CrontabFieldKind Kind { get; }

        /// <summary>
        /// Checks if the value is accepted by the filter
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="kind">The kind of field being evaluated</param>
        /// <returns>True if the value matches the condition, False if it does not match.</returns>
        bool IsMatch(DateTime value);
    }
}
