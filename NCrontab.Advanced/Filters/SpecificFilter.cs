using System;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Extensions;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Filters
{
    /// <summary>
    /// Handles filtering for a specific value
    /// </summary>
    public class SpecificFilter : ICronFilter, ITimeFilter
    {
		private static readonly int MinYear = DateTime.MinValue.Year;
		private static readonly int MaxYear = DateTime.MaxValue.Year;

		public CrontabFieldKind Kind { get; }
        public int SpecificValue { get; private set; }

        /// <summary>
        /// Constructs a new RangeFilter instance
        /// </summary>
        /// <param name="specificValue">The specific value you wish to match</param>
        /// <param name="kind">The crontab field kind to associate with this filter</param>
        public SpecificFilter(int specificValue, CrontabFieldKind kind)
        {
            SpecificValue = specificValue;
            Kind = kind;

	        ValidateBounds(specificValue);
        }

	    private void ValidateBounds(int specificValue)
	    {
		    var minimum = Constants.MinimumDateTimeValues[Kind];
			var maximum = Constants.MaximumDateTimeValues[Kind];

			if (SpecificValue < minimum || SpecificValue > maximum)
				throw new ArgumentOutOfRangeException(nameof(specificValue), $"{nameof(specificValue)} should be between {minimum} and {maximum} (was {SpecificValue})");

		    if (Kind == CrontabFieldKind.DayOfWeek)
		    {
				// This allows Sunday to be represented by both 0 and 7
				SpecificValue = SpecificValue % 7;
			}
		}

	    /// <summary>
        /// Checks if the value is accepted by the filter
        /// </summary>
        /// <param name="value">The value to check</param>
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

            return evalValue == SpecificValue;
        }

        public virtual int? Next(int value)
        {
            return SpecificValue;
        }

        public virtual int? Previous(int value)
        {
            return SpecificValue;
        }

        public int First()
        {
            return SpecificValue;
        }

        public int Last()
        {
            return SpecificValue;
        }


        public override string ToString()
        {
            return SpecificValue.ToString();
        }
    }
}
