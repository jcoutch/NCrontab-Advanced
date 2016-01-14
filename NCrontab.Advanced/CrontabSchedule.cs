using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Extensions;
using NCrontab.Advanced.Filters;
using NCrontab.Advanced.Interfaces;

namespace NCrontab.Advanced
{
    public class CrontabSchedule
    {
        public Dictionary<CrontabFieldKind, List<ICronFilter>> Filters { get; set; }
        public CronStringFormat Format { get; set; }

        // In the event a developer creates their own instance
        public CrontabSchedule()
        {
            Filters = new Dictionary<CrontabFieldKind, List<ICronFilter>>();
            Format = CronStringFormat.Default;
        }

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
            // If no timeout specified, let it run!
            if (timeout <= 0)
                return InternalGetNextOccurence(baseValue, endValue);

            // If a timeout is specified, wait, and if it can't find within the alloted time, throw an exception.
            var task = Task.Factory.StartNew(() => InternalGetNextOccurence(baseValue, endValue));
            var foundValue = task.Wait(timeout);
            if (!foundValue) throw new TimeoutException("GetNextOccurrence timed out while finding next value");

            return task.Result;
        }

        private int Increment(IEnumerable<ITimeFilter> filters, int value, int defaultValue, out bool overflow)
        {
            var nextValue = filters.Select(x => x.Next(value)).Where(x => x > value).Min() ?? defaultValue;
            overflow = nextValue <= value;
            return nextValue;
        }

        private DateTime MinDate(DateTime newValue, DateTime endValue)
        {
            return newValue >= endValue ? endValue : newValue;
        }

        private DateTime InternalGetNextOccurence(DateTime baseValue, DateTime endValue)
        {
            var newValue = baseValue;
            var overflow = true;

            var isSecondFormat = Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears;
            var isYearFormat = Format == CronStringFormat.WithYears || Format == CronStringFormat.WithSecondsAndYears;

            // First things first - trim off any time components we don't need
            newValue = newValue.AddMilliseconds(-newValue.Millisecond);
            if (!isSecondFormat) newValue = newValue.AddSeconds(-newValue.Second);

            var minuteFilters = Filters[CrontabFieldKind.Minute].Where(x => x is ITimeFilter).Cast<ITimeFilter>().ToList();
            var hourFilters = Filters[CrontabFieldKind.Hour].Where(x => x is ITimeFilter).Cast<ITimeFilter>().ToList();

            var firstSecondValue = newValue.Second;
            var firstMinuteValue = minuteFilters.Select(x => x.First()).Min();
            var firstHourValue = hourFilters.Select(x => x.First()).Min();

            var newSeconds = newValue.Second;
            if (isSecondFormat)
            {
                var secondFilters = Filters[CrontabFieldKind.Second].Where(x => x is ITimeFilter).Cast<ITimeFilter>().ToList();
                firstSecondValue = secondFilters.Select(x => x.First()).Min();
                newSeconds = Increment(secondFilters, newValue.Second, firstSecondValue, out overflow);
                newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);
                if (!overflow && !IsMatch(newValue))
                {
                    newSeconds = firstSecondValue;
                    newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);
                    overflow = true;
                }
                if (!overflow) return MinDate(newValue, endValue);
            }

            var newMinutes = Increment(minuteFilters, newValue.Minute + (overflow ? 0 : -1), firstMinuteValue, out overflow);
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, overflow ? firstSecondValue : newSeconds);
            if (!overflow && !IsMatch(newValue))
            {
                newSeconds = firstSecondValue;
                newMinutes = firstMinuteValue;
                newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, firstSecondValue);
                overflow = true;
            }
            if (!overflow) return MinDate(newValue, endValue);

            var newHours = Increment(hourFilters, newValue.Hour + (overflow ? 0 : -1), firstHourValue, out overflow);
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newHours,
                overflow ? firstMinuteValue : newMinutes,
                overflow ? firstSecondValue : newSeconds);

            if (!overflow && !IsMatch(newValue))
            {
                newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, firstHourValue, firstMinuteValue, firstSecondValue);
                overflow = true;
            }

            if (!overflow) return MinDate(newValue, endValue);

            // Sooo, this is where things get more complicated.
            // Since the filtering of days relies on what month/year you're in
            // (for weekday/nth day filters), we'll only increment the day, and
            // check all day/month/year filters.  Might be a litle slow, but we
            // won't miss any days that way.

            // Also, if we increment to the next day, we need to set the hour, minute and second
            // fields to their "first" values, since that would be the earliest they'd run.  We
            // only have to do this after the initial AddDays call.  FYI - they're already at their
            // first values if overflowHour = True.  :-)

            newValue = newValue.AddDays(1);
            while (!(IsMatch(newValue, CrontabFieldKind.Day) && IsMatch(newValue, CrontabFieldKind.DayOfWeek) && IsMatch(newValue, CrontabFieldKind.Month) && (!isYearFormat || IsMatch(newValue, CrontabFieldKind.Year))))
            {
                if (newValue >= endValue) return MinDate(newValue, endValue);

                // This feels so dirty.  This is to catch the odd case where you specify
                // 12/31/9999 23:59:59.999 as your end date, and you don't have any matches,
                // so it reaches the max value of DateTime and throws an exception.
                try { newValue = newValue.AddDays(1); } catch {return endValue; }
            }

            return MinDate(newValue, endValue);
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

        public bool IsMatch(DateTime value)
        {
            return Filters.All(fieldKind =>
                fieldKind.Value.Any(filter => filter.IsMatch(value))
            );
        }

        public bool IsMatch(DateTime value, CrontabFieldKind kind)
        {
            return Filters.Where(x => x.Key == kind).SelectMany(x => x.Value).Any(filter => filter.IsMatch(value));
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

        public static CrontabSchedule Parse(string expression, CronStringFormat format = CronStringFormat.Default)
        {
            return new CrontabSchedule
            {
                Format = format,
                Filters = ParseToDictionary(expression, format)
            };
        }

        public static CrontabSchedule TryParse(string expression, CronStringFormat format = CronStringFormat.Default)
        {
            try
            {
                return Parse(expression, format);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void CheckForIllegalFilters(Dictionary<CrontabFieldKind, List<ICronFilter>> filters)
        {
            var monthSingle = GetSpecificFilters(filters, CrontabFieldKind.Month);
            var daySingle = GetSpecificFilters(filters, CrontabFieldKind.Day);

            if (monthSingle.Any() && monthSingle.All(x => x.SpecificValue == 2))
            {
                if (daySingle.Any() && daySingle.All(x => (x.SpecificValue == 30) || (x.SpecificValue == 31)))
                    throw new CrontabException("Nice try, but February 30 and 31 don't exist.");
            }
        }

        private static List<SpecificFilter> GetSpecificFilters(Dictionary<CrontabFieldKind, List<ICronFilter>> filters, CrontabFieldKind kind)
        {
            return filters[kind].Where(x => x.GetType() == typeof(SpecificFilter)).Union(
                filters[kind].Where(x => x.GetType() == typeof(RangeFilter)).SelectMany(x => ((RangeFilter)x).SpecificFilters)
                ).Union(
                    filters[kind].Where(x => x.GetType() == typeof(StepFilter)).SelectMany(x => ((StepFilter)x).SpecificFilters)
                ).Cast<SpecificFilter>().ToList();
        }

        private static Dictionary<CrontabFieldKind, List<ICronFilter>> ParseToDictionary(string cron, CronStringFormat format)
        {
            if (string.IsNullOrWhiteSpace(cron))
                throw new CrontabException("The provided cron string is null, empty or contains only whitespace");

            var fields = new Dictionary<CrontabFieldKind, List<ICronFilter>>();

            var instructions = cron.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);

            var expectedCount = Constants.ExpectedFieldCounts[format];
            if (instructions.Length > expectedCount)
                throw new CrontabException(string.Format("The provided cron string <{0}> has too many parameters", cron));
            if (instructions.Length < expectedCount)
                throw new CrontabException(string.Format("The provided cron string <{0}> has too few parameters", cron));

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

            CheckForIllegalFilters(fields);

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
                throw new CrontabException(string.Format("There was an error parsing '{0}' for the {1} field", field, Enum.GetName(typeof(CrontabFieldKind), kind)), e);
            }
        }

        private static ICronFilter ParseFilter(string filter, CrontabFieldKind kind)
        {
            var newFilter = filter.ToUpper();

            try
            {
                if (newFilter.StartsWith("*", StringComparison.OrdinalIgnoreCase))
                {
                    newFilter = newFilter.Substring(1);
                    if (newFilter.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        newFilter = newFilter.Substring(1);
                        var steps = GetValue(ref newFilter, kind);
                        return new StepFilter(0, steps, kind);
                    }
                    return new AnyFilter(kind);
                }

                if (newFilter.StartsWith("L") && kind == CrontabFieldKind.Day)
                {
                    newFilter = newFilter.Substring(1);
                    if (newFilter == "W")
                        return new LastWeekdayOfMonthFilter(kind);
                    else
                        return new LastDayOfMonthFilter(kind);
                }

                var firstValue = GetValue(ref newFilter, kind);

                if (string.IsNullOrEmpty(newFilter))
                    return new SpecificFilter(firstValue, kind);

                switch (newFilter[0])
                {
                    case '/':
                        {
                            newFilter = newFilter.Substring(1);
                            var secondValue = GetValue(ref newFilter, kind);
                            return new StepFilter(firstValue, secondValue, kind);
                        }
                    case '-':
                    {
                        newFilter = newFilter.Substring(1);
                        var secondValue = GetValue(ref newFilter, kind);
                        int? steps = null;
                        if (newFilter.StartsWith("/"))
                        {
                            newFilter = newFilter.Substring(1);
                            steps = GetValue(ref newFilter, kind);
                        }
                        return new RangeFilter(firstValue, secondValue, steps, kind);
                    }
                    case '#':
                    {
                        newFilter = newFilter.Substring(1);
                        var secondValue = GetValue(ref newFilter, kind);

                        if (!string.IsNullOrEmpty(newFilter))
                            throw new CrontabException(string.Format("Invalid filter '{0}'", filter));

                        return new SpecificDayOfWeekInMonthFilter(firstValue, secondValue, kind);
                    }
                    default:
                        if (newFilter == "L" && kind == CrontabFieldKind.DayOfWeek)
                        {
                            return new LastDayOfWeekInMonthFilter(firstValue, kind);
                        }
                        else if (newFilter == "W" && kind == CrontabFieldKind.Day)
                        {
                            return new NearestWeekdayFilter(firstValue, kind);
                        }
                        break;
                }

                throw new CrontabException(string.Format("Invalid filter '{0}'", filter));
            }
            catch (Exception e)
            {
                throw new CrontabException(string.Format("Invalid filter '{0}'.  See inner exception for details.", filter), e);
            }
        }

        private static int GetValue(ref string filter, CrontabFieldKind kind)
        {
            var maxValue = Constants.MaximumDateTimeValues[kind];

            if (string.IsNullOrEmpty(filter))
                throw new CrontabException("Expected number, but filter was empty.");

            int i, value;
            var isDigit = char.IsDigit(filter[0]);
            var isLetter = char.IsLetter(filter[0]);

            // Because this could either numbers, or letters, but not a combination,
            // check each condition separately.
            for (i = 0; i < filter.Length; i++)
                if ((isDigit && !char.IsDigit(filter[i])) || (isLetter && !char.IsLetter(filter[i]))) break;

            var valueToParse = filter.Substring(0, i);
            if (int.TryParse(valueToParse, out value))
            {
                filter = filter.Substring(i);
                var returnValue = value;
                if (returnValue > maxValue)
                    throw new CrontabException(string.Format("Value for {0} filter exceeded maximum value of {1}", Enum.GetName(typeof(CrontabFieldKind), kind), maxValue));
                return returnValue;
            }
            else
            {
                List<KeyValuePair<string, int>> replaceVal = null;

                if (kind == CrontabFieldKind.DayOfWeek)
                    replaceVal = Constants.Days.Where(x => valueToParse.StartsWith(x.Key)).ToList();
                else if (kind == CrontabFieldKind.Month)
                    replaceVal = Constants.Months.Where(x => valueToParse.StartsWith(x.Key)).ToList();

                if (replaceVal != null && replaceVal.Count == 1)
                {
                    filter = filter.Substring(i);
                    var returnValue = replaceVal.First().Value;
                    if (returnValue > maxValue)
                        throw new CrontabException(string.Format("Value for {0} filter exceeded maximum value of {1}", Enum.GetName(typeof(CrontabFieldKind), kind), maxValue));
                    return returnValue;
                }
            }

            throw new CrontabException("Filter does not contain expected number");
        }

        #endregion
    }
}
