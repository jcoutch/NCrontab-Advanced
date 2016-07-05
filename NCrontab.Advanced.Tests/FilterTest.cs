using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Filters;
using NCrontab.Advanced.Tests.Extensions;

namespace NCrontab.Advanced.Tests
{
    [TestClass]
    public class FilterTest
    {
        #region LastDayOfMonthFilter tests

        [TestMethod]
        public void LastDayOfMonthFilterWorks()
        {
            var tests = new Dictionary<DateTime, bool>
            {
                {new DateTime(2016, 1, 1), false},
                {new DateTime(2016, 1, 30), false},
                {new DateTime(2016, 1, 31), true},
                {new DateTime(2015, 2, 28), true},
                {new DateTime(2016, 2, 28), false},
                {new DateTime(2016, 2, 29), true},
                {new DateTime(2016, 4, 1), false},
                {new DateTime(2016, 4, 29), false},
                {new DateTime(2016, 4, 30), true},
                {new DateTime(2016, 12, 1), false},
                {new DateTime(2016, 12, 30), false},
                {new DateTime(2016, 12, 31), true},
            };

            var method = new LastDayOfMonthFilter(CrontabFieldKind.Day);
            foreach (var pair in tests)
                Assert.AreEqual(pair.Value, method.IsMatch(pair.Key), "Is {0} the last day of the month");
        }

        [TestMethod]
        public void LastDayOfMonthFilterInvalidState()
        {
            var values = Enum.GetValues(typeof(CrontabFieldKind)).Cast<CrontabFieldKind>().Where(x => x != CrontabFieldKind.Day);

            foreach (var type in values)
                Assert2.Throws<CrontabException>(() => new LastDayOfMonthFilter(type), "Ensure LastDayOfMonthFilter can't be instantiated with <{0}>", Enum.GetName(typeof(CrontabFieldKind), type));
        }

        #endregion

        #region LastDayOfWeekInMonthFilter tests

        [TestMethod]
        public void LastDayOfWeekInMonthFilterTest()
        {
            var tests = new[]
            {
                new { day = DayOfWeek.Sunday,    output = new DateTime(2016, 1, 31)},
                new { day = DayOfWeek.Monday,    output = new DateTime(2016, 1, 25)},
                new { day = DayOfWeek.Tuesday,   output = new DateTime(2016, 1, 26)},
                new { day = DayOfWeek.Wednesday, output = new DateTime(2016, 1, 27)},
                new { day = DayOfWeek.Thursday,  output = new DateTime(2016, 1, 28)},
                new { day = DayOfWeek.Friday,    output = new DateTime(2016, 1, 29)},
                new { day = DayOfWeek.Saturday,  output = new DateTime(2016, 1, 30)},
            };

            foreach (var test in tests)
            {
                var method = new LastDayOfWeekInMonthFilter(Constants.CronDays[test.day], CrontabFieldKind.DayOfWeek);
                Assert.IsTrue(method.IsMatch(test.output), "Is {0} the last instance of that day in the month");
            }
        }

        [TestMethod]
        public void LastDayOfWeekInMonthFilterInvalidState()
        {
            var values = Enum.GetValues(typeof(CrontabFieldKind)).Cast<CrontabFieldKind>().Where(x => x != CrontabFieldKind.DayOfWeek);

            foreach (var type in values)
                Assert2.Throws<CrontabException>(() => new LastDayOfWeekInMonthFilter(0, type), "Ensure LastDayOfWeekInMonthFilter can't be instantiated with <{0}>", Enum.GetName(typeof(CrontabFieldKind), type));
        }

        #endregion

        #region LastWeekdayOfMonthFilter tests

        [TestMethod]
        public void LastWeekdayOfMonthFilterTest()
        {
            var tests = new[]
            {
                new { output = new DateTime(2015, 2, 27)},
                new { output = new DateTime(2016, 1, 29)},
                new { output = new DateTime(2016, 2, 29)},
                new { output = new DateTime(2016, 3, 31)},
                new { output = new DateTime(2016, 4, 29)},
                new { output = new DateTime(2016, 5, 31)},
                new { output = new DateTime(2016, 6, 30)},
                new { output = new DateTime(2016, 7, 29)},
                new { output = new DateTime(2016, 8, 31)},
                new { output = new DateTime(2016, 9, 30)},
                new { output = new DateTime(2016, 10, 31)},
                new { output = new DateTime(2016, 11, 30)},
                new { output = new DateTime(2016, 12, 30)},
            };

            var method = new LastWeekdayOfMonthFilter(CrontabFieldKind.Day);
            foreach (var test in tests)
                Assert.IsTrue(method.IsMatch(test.output), "Is {0} the last week day in a month");
        }

        [TestMethod]
        public void LastWeekdayOfMonthFilterInvalidState()
        {
            var values = Enum.GetValues(typeof(CrontabFieldKind)).Cast<CrontabFieldKind>().Where(x => x != CrontabFieldKind.Day);

            foreach (var type in values)
                Assert2.Throws<CrontabException>(() => new LastWeekdayOfMonthFilter(type), "Ensure LastWeekdayOfMonth can't be instantiated with <{0}>", Enum.GetName(typeof(CrontabFieldKind), type));
        }

        #endregion

        #region NearestWeekdayFilter tests

        [TestMethod]
        public void NearestWeekdayFilterTest()
        {
            var tests = new[]
            {
                new { day =  1, output = new DateTime(2015, 1, 1)},
                new { day =  2, output = new DateTime(2016, 1, 1)},
                new { day =  3, output = new DateTime(2016, 1, 4)},
                new { day =  4, output = new DateTime(2016, 1, 4)},
                new { day = 29, output = new DateTime(2016, 1, 29)},
                new { day = 30, output = new DateTime(2016, 1, 29)},
                new { day = 31, output = new DateTime(2016, 1, 29)},
                new { day = 1, output = new DateTime(2016, 10, 3)},
                new { day = 2, output = new DateTime(2016, 10, 3)},
                new { day = 3, output = new DateTime(2016, 10, 3)},
            };

            foreach (var test in tests)
            {
                var method = new NearestWeekdayFilter(test.day, CrontabFieldKind.Day);
                Assert.IsTrue(method.IsMatch(test.output), "Is {0} the nearest weekday for day = {1}", test.output, test.day);
            }
        }

        [TestMethod]
        public void NearestWeekdayFilterInvalidState()
        {
            var values = Enum.GetValues(typeof(CrontabFieldKind)).Cast<CrontabFieldKind>().Where(x => x != CrontabFieldKind.Day);

            foreach (var type in values)
                Assert2.Throws<CrontabException>(() => new NearestWeekdayFilter(1, type), "Ensure NearestWeekdayFilter can't be instantiated with <{0}>", Enum.GetName(typeof(CrontabFieldKind), type));

            Assert2.Throws<CrontabException>(() => new NearestWeekdayFilter(-1, CrontabFieldKind.Day));
            Assert2.Throws<CrontabException>(() => new NearestWeekdayFilter(32, CrontabFieldKind.Day));
        }

        #endregion

        #region RangeFilter tests

        [TestMethod]
        public void RangeFilterTest()
        {
            var tests = new[]
            {
                new { start = 5, end = 8, steps = (int?) null, input = new DateTime(2015, 1, 4), result = false },
                new { start = 5, end = 8, steps = (int?) null, input = new DateTime(2015, 1, 5), result = true },
                new { start = 5, end = 8, steps = (int?) null, input = new DateTime(2015, 1, 6), result = true },
                new { start = 5, end = 8, steps = (int?) null, input = new DateTime(2015, 1, 7), result = true },
                new { start = 5, end = 8, steps = (int?) null, input = new DateTime(2015, 1, 8), result = true },
                new { start = 5, end = 8, steps = (int?) null, input = new DateTime(2015, 1, 9), result = false },

                new { start = 5, end = 8, steps = (int?) 2, input = new DateTime(2015, 1, 4), result = false },
                new { start = 5, end = 8, steps = (int?) 2, input = new DateTime(2015, 1, 5), result = true },
                new { start = 5, end = 8, steps = (int?) 2, input = new DateTime(2015, 1, 6), result = false },
                new { start = 5, end = 8, steps = (int?) 2, input = new DateTime(2015, 1, 7), result = true },
                new { start = 5, end = 8, steps = (int?) 2, input = new DateTime(2015, 1, 8), result = false },
                new { start = 5, end = 8, steps = (int?) 2, input = new DateTime(2015, 1, 9), result = false },
            };

            foreach (var test in tests)
            {
                var method = new RangeFilter(test.start, test.end, test.steps, CrontabFieldKind.Day);
                Assert.AreEqual(test.result, method.IsMatch(test.input), "Is {0} in the range of {1}-{2}/{3}?", test.input, test.start, test.end, test.steps ?? 1);
            }
        }

        [TestMethod]
        public void RangeFilterInvalidState()
        {
            Assert2.Throws<CrontabException>(() => new RangeFilter(-1, 1, null, CrontabFieldKind.Day));
            Assert2.Throws<CrontabException>(() => new RangeFilter(1, -1, null, CrontabFieldKind.Day));
            Assert2.Throws<CrontabException>(() => new RangeFilter(1, 1, -1, CrontabFieldKind.Day));
            Assert2.Throws<CrontabException>(() => new RangeFilter(1, 1, 0, CrontabFieldKind.Day));

            Assert2.Throws<CrontabException>(() => new RangeFilter(32, 1, null, CrontabFieldKind.Day));
            Assert2.Throws<CrontabException>(() => new RangeFilter(1, 32, null, CrontabFieldKind.Day));
            Assert2.Throws<CrontabException>(() => new RangeFilter(1, 1, 32, CrontabFieldKind.Day));
        }

        #endregion

        #region SpecificDayOfWeekInMonthFilter tests

        [TestMethod]
        public void SpecificDayOfWeekInMonthFilterTest()
        {
            var tests = new[]
            {
                new { day = DayOfWeek.Sunday,    week = 1, output = new DateTime(2016, 1, 3)},
                new { day = DayOfWeek.Monday,    week = 1, output = new DateTime(2016, 1, 4)},
                new { day = DayOfWeek.Tuesday,   week = 1, output = new DateTime(2016, 1, 5)},
                new { day = DayOfWeek.Wednesday, week = 1, output = new DateTime(2016, 1, 6)},
                new { day = DayOfWeek.Thursday,  week = 1, output = new DateTime(2016, 1, 7)},
                new { day = DayOfWeek.Friday,    week = 1, output = new DateTime(2016, 1, 1)},
                new { day = DayOfWeek.Saturday,  week = 1, output = new DateTime(2016, 1, 2)},
                new { day = DayOfWeek.Sunday,    week = 2, output = new DateTime(2016, 1, 10)},
                new { day = DayOfWeek.Monday,    week = 2, output = new DateTime(2016, 1, 11)},
                new { day = DayOfWeek.Tuesday,   week = 2, output = new DateTime(2016, 1, 12)},
                new { day = DayOfWeek.Wednesday, week = 2, output = new DateTime(2016, 1, 13)},
                new { day = DayOfWeek.Thursday,  week = 2, output = new DateTime(2016, 1, 14)},
                new { day = DayOfWeek.Friday,    week = 2, output = new DateTime(2016, 1, 8)},
                new { day = DayOfWeek.Saturday,  week = 2, output = new DateTime(2016, 1, 9)},
            };

            foreach (var test in tests)
            {
                var method = new SpecificDayOfWeekInMonthFilter(Constants.CronDays[test.day], test.week, CrontabFieldKind.DayOfWeek);
                Assert.IsTrue(method.IsMatch(test.output), "Is {0} instance number {1} of {2}?", test.output, test.week, Enum.GetName(typeof(DayOfWeek), test.day));
            }
        }

        [TestMethod]
        public void SpecificDayOfWeekInMonthFilterInvalidState()
        {
            var values = Enum.GetValues(typeof(CrontabFieldKind)).Cast<CrontabFieldKind>().Where(x => x != CrontabFieldKind.DayOfWeek);

            foreach (var type in values)
                Assert2.Throws<CrontabException>(() => new SpecificDayOfWeekInMonthFilter(0, 1, type), "Ensure SpecificDayOfWeekInMonthFilter can't be instantiated with <{0}>", Enum.GetName(typeof(CrontabFieldKind), type));

            Assert2.Throws<CrontabException>(() => new SpecificDayOfWeekInMonthFilter(0, -1, CrontabFieldKind.DayOfWeek), "Make sure instance of -1 throws exception");
            Assert2.Throws<CrontabException>(() => new SpecificDayOfWeekInMonthFilter(0, 0, CrontabFieldKind.DayOfWeek), "Make sure instance of 0 throws exception");
            Assert2.Throws<CrontabException>(() => new SpecificDayOfWeekInMonthFilter(0, 6, CrontabFieldKind.DayOfWeek), "Makes sure instance of 6 throws exception");
        }

        #endregion

        #region SpecificFilter tests

        [TestMethod]
        public void SpecificFilterTest()
        {
            var tests = new[]
            {
                new { specific = 6, output = new DateTime(2016, 1, 5), kind = CrontabFieldKind.Day, isMatch = false },
                new { specific = 6, output = new DateTime(2016, 1, 6), kind = CrontabFieldKind.Day, isMatch = true },
                new { specific = 6, output = new DateTime(2016, 1, 7), kind = CrontabFieldKind.Day, isMatch = false },
            };

            foreach (var test in tests)
            {
                var method = new SpecificFilter(test.specific, test.kind);
                Assert.AreEqual(test.isMatch, method.IsMatch(test.output), "Is {0} a match to specific {1} of {2}?", test.output, Enum.GetName(typeof(CrontabFieldKind), test.kind), test.specific);
            }
        }

        #endregion

        #region StepFilter tests

        [TestMethod]
        public void StepFilterTest()
        {
            var tests = new[]
            {
                new { start = 5, step = 3, input = new DateTime(2016, 1, 1), isMatch = false },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 2), isMatch = true },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 3), isMatch = false },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 4), isMatch = false },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 5), isMatch = true },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 6), isMatch = false },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 7), isMatch = false },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 8), isMatch = true },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 9), isMatch = false },
                new { start = 5, step = 3, input = new DateTime(2016, 1, 10), isMatch = false },
            };

            foreach (var test in tests)
            {
                var method = new StepFilter(test.start, test.step, CrontabFieldKind.Day);
                Assert.AreEqual(test.isMatch, method.IsMatch(test.input), "Is {0} a match to {1}/{2}?", test.input, test.start, test.step);
            }
        }

        #endregion

        [TestMethod]
        public void BlankDayOfMonthOrWeekFilterInvalidState()
        {
            var values = new CrontabFieldKind[] {
                CrontabFieldKind.Hour,
                CrontabFieldKind.Minute,
                CrontabFieldKind.Month,
                CrontabFieldKind.Second,
                CrontabFieldKind.Year
            };

            foreach (var val in values)
                Assert2.Throws<CrontabException>(() => new BlankDayOfMonthOrWeekFilter(val), "Ensure BlankDayOfMonthOrWeekFilter can't be instantiated with <{0}>", Enum.GetName(typeof(CrontabFieldKind), val));

            Assert.IsTrue(new BlankDayOfMonthOrWeekFilter(CrontabFieldKind.Day).IsMatch(DateTime.Now));
            Assert.IsTrue(new BlankDayOfMonthOrWeekFilter(CrontabFieldKind.DayOfWeek).IsMatch(DateTime.UtcNow));
        }
    }
}
