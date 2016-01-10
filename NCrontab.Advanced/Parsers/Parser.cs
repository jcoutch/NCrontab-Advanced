using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Filters;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced.Parsers
{
    public class CronInstance
    {
        private static readonly Dictionary<string, string> ReplaceValues = new Dictionary<string, string>
        {
            {"JAN", "1"},
            {"FEB", "2"},
            {"MAR", "3"},
            {"APR", "4"},
            {"MAY", "5"},
            {"JUN", "6"},
            {"JUL", "7"},
            {"AUG", "8"},
            {"SEP", "9"},
            {"OCT", "10"},
            {"NOV", "11"},
            {"DEC", "12"},
            {"SUN", "0"},
            {"MON", "1"},
            {"TUE", "2"},
            {"WED", "3"},
            {"THU", "4"},
            {"FRI", "5"},
            {"SAT", "6"},
        };

        public Dictionary<CrontabFieldKind, List<ICronFilter>> Filters { get; set; }
        public CronStringFormat Format { get; set; }

        public override string ToString()
        {
            var paramList = new List<string>();

            if (Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears)
                JoinFilters(paramList, CrontabFieldKind.Second);

            JoinFilters(paramList, CrontabFieldKind.Minute);
            JoinFilters(paramList, CrontabFieldKind.Hour);
            JoinFilters(paramList, CrontabFieldKind.Day);
            JoinFilters(paramList, CrontabFieldKind.Month);
            JoinFilters(paramList, CrontabFieldKind.DayOfWeek);

            if (Format == CronStringFormat.WithYears || Format == CronStringFormat.WithSecondsAndYears)
                JoinFilters(paramList, CrontabFieldKind.Second);

            return string.Join(" ", paramList);
        }

        public DateTime GetNextOccurrence(DateTime baseValue, int timeout = 0)
        {
            return GetNextOccurrence(baseValue, DateTime.MaxValue, timeout);
        }

        public DateTime GetNextOccurrence(DateTime baseValue, DateTime endValue, int timeout = 0)
        {
            var task = new Task<DateTime>(() => InternalGetNextOccurence(baseValue, endValue));

            // If no timeout specified, let it run!
            if (timeout == 0) return task.Result;

            // If a timeout is specified, wait, and if it can't find within the alloted time, throw an exception.
            var foundValue = task.Wait(timeout);
            if (!foundValue) throw new TimeoutException("GetNextOccurrence timed out while finding next value");

            return task.Result;
        }

        private DateTime InternalGetNextOccurence(DateTime baseValue, DateTime endValue)
        {
            var newValue = baseValue;
            bool overflow = false, foundValue = false;

            // Increment seconds
            if (Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears)
            {
                newValue = DateIncrementer(newValue,
                    x => x.Second,
                    x => x.AddSeconds(1),
                    CrontabFieldKind.Second, true, out foundValue, out overflow);

                if (newValue >= endValue) return endValue;
                if (!overflow && foundValue) return newValue;
            }

            // Increment minutes
            newValue = DateIncrementer(newValue,
                x => x.Minute,
                x => x.AddMinutes(1),
                CrontabFieldKind.Minute, overflow, out foundValue, out overflow);

            if (newValue >= endValue) return endValue;
            if (!overflow && foundValue) return newValue;

            // Increment hours
            newValue = DateIncrementer(newValue,
                x => x.Hour,
                x => x.AddHours(1),
                CrontabFieldKind.Hour, overflow, out foundValue, out overflow);

            if (newValue >= endValue) return endValue;
            if (!overflow && foundValue) return newValue;

            // Sooo, this is where things get more complicated.
            // Since the filtering of days relies on what month/year you're in
            // (for weekday/nth day filters), we'll only increment the day, and
            // check all day/month/year filters.  Might be a litle slow, but we
            // won't miss any days that way.

            while (!(IsMatch(newValue, CrontabFieldKind.Day) && IsMatch(newValue, CrontabFieldKind.DayOfWeek) && IsMatch(newValue, CrontabFieldKind.Month) && IsMatch(newValue, CrontabFieldKind.Year)))
            {
                newValue = newValue.AddDays(1);
                if (newValue >= endValue) return endValue;
            }

            return newValue;
        }

        private DateTime DateIncrementer(DateTime baseValue, Func<DateTime, int> valueFunc, Func<DateTime, DateTime> incrementFunc, CrontabFieldKind kind, bool includeCurrentValue, out bool foundValue, out bool overflow)
        {
            var newValue = baseValue;
            overflow = false;
            foundValue = false;
            while (overflow == false || valueFunc(newValue) != valueFunc(baseValue))
            {
                if (!includeCurrentValue)
                    newValue = incrementFunc(newValue);
                else
                    includeCurrentValue = false;

                if (valueFunc(newValue) == 0) overflow = true;
                if (IsMatch(newValue, kind))
                {
                    foundValue = true;
                    break;
                }
            }
            return newValue;
        }

        public IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, DateTime endTime)
        {
            for (var occurrence = GetNextOccurrence(baseTime, endTime);
                 occurrence < endTime;
                 occurrence = GetNextOccurrence(occurrence, endTime))
            {
                yield return occurrence;
            }
        }

        private bool IsMatch(DateTime value, CrontabFieldKind kind)
        {
            return Filters.Where(x => x.Key == kind).All(fieldKind =>
                fieldKind.Value.Any(filter => filter.IsMatch(value))
            );
        }

        public bool IsMatch(DateTime value)
        {
            return Filters.All(fieldKind =>
                fieldKind.Value.Any(filter => filter.IsMatch(value))
            );
        }

        private DateTime IncrementDate(DateTime value)
        {
            if (Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears)
                return value.AddSeconds(1);

            return value.AddMinutes(1);
        }

        private void JoinFilters(List<string> paramList, CrontabFieldKind kind)
        {
            paramList.Add(
                string.Join(",", Filters
                    .Where(x => x.Key == kind)
                    .SelectMany(x => x.Value.Select(y => y.ToString()))
                )
            );
        }


        #region Static Methods

        public static CronInstance Parse(string cronString, CronStringFormat format = CronStringFormat.Default)
        {
            return new CronInstance
            {
                Filters = ParseToDictionary(cronString, format)
            };
        }

        private static Dictionary<CrontabFieldKind, List<ICronFilter>> ParseToDictionary(string cron, CronStringFormat format)
        {
            var fields = new Dictionary<CrontabFieldKind, List<ICronFilter>>();

            if (fields.Count != GetExpectedFieldCount(format))
                throw new ArgumentException("The provided cron string has too many parameters", nameof(cron));

            var instructions = cron.Split(' ');

            var defaultFieldOffset = 0;
            if (format == CronStringFormat.WithSeconds || format == CronStringFormat.WithSecondsAndYears)
            {
                fields.Add(CrontabFieldKind.Second, ParseField(instructions[0], CrontabFieldKind.Second));
                defaultFieldOffset = 1;
            }

            fields.Add(CrontabFieldKind.Minute, ParseField(instructions[defaultFieldOffset + 0], CrontabFieldKind.Minute));
            fields.Add(CrontabFieldKind.Hour, ParseField(instructions[defaultFieldOffset + 1], CrontabFieldKind.Hour));
            fields.Add(CrontabFieldKind.Day, ParseField(instructions[defaultFieldOffset + 2], CrontabFieldKind.Day));
            fields.Add(CrontabFieldKind.Month, ParseField(instructions[defaultFieldOffset + 3], CrontabFieldKind.Month));
            fields.Add(CrontabFieldKind.DayOfWeek, ParseField(instructions[defaultFieldOffset + 4], CrontabFieldKind.DayOfWeek));

            if (format == CronStringFormat.WithYears || format == CronStringFormat.WithSecondsAndYears)
                fields.Add(CrontabFieldKind.Year, ParseField(instructions[defaultFieldOffset + 5], CrontabFieldKind.Year));

            return fields;
        }

        private static List<ICronFilter> ParseField(string field, CrontabFieldKind kind)
        {
            try
            {
                return field.Split(',').Select(filter => ParseFilter(filter, kind)).ToList();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("There was an error parsing '{0}' for the {1} field", field, Enum.GetName(typeof(CrontabFieldKind), kind)));
            }
        }

        private static ICronFilter ParseFilter(string filter, CrontabFieldKind kind)
        {
            // Replace all instances of text-based months/days with numbers
            var newFilter = ReplaceValues.Aggregate(filter, (current, value) => current.Replace(value.Key, value.Value));

            try
            {
                if (newFilter == "*") return new AnyFilter(kind);
                if (newFilter == "L" && kind == CrontabFieldKind.Day) return new LastDayOfMonthFilter(kind);

                var firstValue = GetValue(ref newFilter);

                if (string.IsNullOrEmpty(newFilter))
                    return new SpecificFilter(firstValue, kind);

                switch (newFilter[0])
                {
                    case '-':
                    {
                        newFilter = newFilter.Substring(1);
                        var secondValue = GetValue(ref newFilter);
                        return new RangeFilter(firstValue, secondValue, kind);
                    }
                    case '#':
                    {
                        newFilter = newFilter.Substring(1);
                        var secondValue = GetValue(ref newFilter);

                        if (!string.IsNullOrEmpty(newFilter))
                            throw new Exception(string.Format("Invalid filter '{0}'", filter));

                        return new SpecificDayOfWeekInMonthFilter(firstValue, secondValue, kind);
                    }
                    default:
                        if (newFilter == "L" && kind == CrontabFieldKind.DayOfWeek)
                        {
                            return new LastDayOfWeekInMonthFilter(firstValue, kind);
                        }
                        else if (newFilter == "W" && kind == CrontabFieldKind.DayOfWeek)
                        {
                            return new NearestWeekdayFilter(firstValue, kind);
                        }
                        break;
                }

                throw new Exception(string.Format("Invalid filter '{0}'", filter));
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Invalid filter '{0}'.  See inner exception for details.", filter), e);
            }
        }

        private static int GetValue(ref string filter)
        {
            int i, value;
            for (i = 0; i < filter.Length; i++)
                if (!char.IsDigit(filter[i])) break;

            if (int.TryParse(filter.Substring(0, i + 1), out value))
            {
                filter = filter.Substring(i + 1);
                return value;
            }

            throw new Exception("Filter does not contain expected number");
        }

        private static int GetExpectedFieldCount(CronStringFormat format)
        {
            int fieldCount;
            switch (format)
            {
                case CronStringFormat.Default: fieldCount = 5; break;
                case CronStringFormat.WithYears: fieldCount = 6; break;
                case CronStringFormat.WithSeconds: fieldCount = 6; break;
                case CronStringFormat.WithSecondsAndYears: fieldCount = 7; break;
                default: throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
            return fieldCount;
        }

        #endregion
    }
}
