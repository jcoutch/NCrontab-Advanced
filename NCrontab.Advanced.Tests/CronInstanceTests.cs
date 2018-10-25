#region License and Terms
//
// Most unit tests are from NCrontab - Crontab for .NET, written by Atif Aziz.
// Ported to Microsoft's unit test framework by Joe Coutcher
// Project can be accessed at https://github.com/atifaziz/NCrontab
//
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Tests.Extensions;

namespace NCrontab.Advanced.Tests
{

    [TestClass]
    public sealed class CronInstanceTests
    {
        const string TimeFormat = "dd/MM/yyyy HH:mm:ss";

        [TestMethod]
        public void CannotParseNullString()
        {
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse(null));
        }

        [TestMethod]
        public void CannotParseEmptyString()
        {
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse(string.Empty));
        }

        [TestMethod]
        public void AllTimeString()
        {
            var input = "* * * * *";
            var output = CrontabSchedule.Parse(input, CronStringFormat.Default).ToString();
            Assert.AreEqual(input, output);
        }

        [TestMethod]
        public void SixPartAllTimeString()
        {
            Assert.AreEqual("* * * * * *", CrontabSchedule.Parse("* * * * * *", CronStringFormat.WithSeconds).ToString());
        }

        [TestMethod]
        public void InvalidPatternCount()
        {
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * *", CronStringFormat.Default));
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * * *", CronStringFormat.Default));

            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * *", CronStringFormat.WithSeconds));
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * * * *", CronStringFormat.WithSeconds));

            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * *", CronStringFormat.WithYears));
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * * * *", CronStringFormat.WithYears));

            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * * *", CronStringFormat.WithSecondsAndYears));
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * * * * *", CronStringFormat.WithSecondsAndYears));
        }

	    [TestMethod]
	    public void OutOfBoundsValues()
	    {
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("-1 * * * * *", CronStringFormat.WithSeconds));
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("60 * * * * *", CronStringFormat.WithSeconds));

			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * * 0", CronStringFormat.WithYears));
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * * 10000", CronStringFormat.WithYears));

			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("-1 * * * *", CronStringFormat.Default));
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("60 * * * *", CronStringFormat.Default));

			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* -1 * * *", CronStringFormat.Default));
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* 24 * * *", CronStringFormat.Default));

			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * 0 * *", CronStringFormat.Default));
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * 32 * *", CronStringFormat.Default));

			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * 0 *", CronStringFormat.Default));
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * 13 *", CronStringFormat.Default));

			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * -1", CronStringFormat.Default));
			Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * 8", CronStringFormat.Default));
		}

	    [TestMethod]
	    public void SundayProcessesCorrectly()
	    {
			var tests = new[]
			{
				new { startTime = "01/01/2016 00:00:00", inputString = "0 0 * * 0", nextOccurence = "03/01/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
				new { startTime = "01/01/2016 00:00:00", inputString = "0 0 * * 7", nextOccurence = "03/01/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
			};

			foreach (var test in tests)
				CronCall(test.startTime, test.inputString, test.nextOccurence, test.cronStringFormat);
		}

        [TestMethod]
        public void ThirtyFirstWeekdayForMonthsWithLessThanThirtyDaysProcessesCorrectly()
        {
            var tests = new[]
            {
                new { startTime = "31/03/2016 00:00:00", inputString = "0 0 31W * *", nextOccurence = "31/05/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2016 00:00:00", inputString = "0 0 31W * *", nextOccurence = "29/01/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2016 00:00:00", inputString = "0 0 31W * *", nextOccurence = "31/03/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2016 00:00:00", inputString = "0 0 30W * *", nextOccurence = "30/03/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2016 00:00:00", inputString = "0 0 29W * *", nextOccurence = "29/02/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2017 00:00:00", inputString = "0 0 29W * *", nextOccurence = "29/03/2017 00:00:00", cronStringFormat = CronStringFormat.Default },
            };

            foreach (var test in tests)
                CronCall(test.startTime, test.inputString, test.nextOccurence, test.cronStringFormat);
        }

        [TestMethod]
        public void CannotParseWhenSecondsRequired()
        {
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse("* * * * *", CronStringFormat.WithSeconds));
        }

        [TestMethod]
        public void Formatting()
        {
            var tests = new[] {
                new { inputString = "* 1-2,3 * * *"                   , outputString = "* 1-2,3 * * *"                   , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "* * * */2 *"                     , outputString = "* * * */2 *"                     , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "10-40/15 * * * *"                , outputString = "10-40/15 * * * *"                , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "* * * Mar,Jan,Aug Fri,Mon-Tue"   , outputString = "* * * 3,1,8 5,1-2"               , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "1 * 1-2,3 * * *"                 , outputString = "1 * 1-2,3 * * *"                 , cronStringFormat = CronStringFormat.WithSeconds },
	            new { inputString = "22 * * * */2 *"                  , outputString = "22 * * * */2 *"                  , cronStringFormat = CronStringFormat.WithSeconds },
	            new { inputString = "33 10-40/15 * * * *"             , outputString = "33 10-40/15 * * * *"             , cronStringFormat = CronStringFormat.WithSeconds },
	            new { inputString = "55 * * * Mar,Jan,Aug Fri,Mon-Tue", outputString = "55 * * * 3,1,8 5,1-2"            , cronStringFormat = CronStringFormat.WithSeconds },
            };

            foreach (var test in tests)
                Assert.AreEqual(test.outputString, CrontabSchedule.Parse(test.inputString, test.cronStringFormat).ToString());
        }

        /// <summary>
        /// Tests to see if the cron class can calculate the previous matching
        /// time correctly in various circumstances.
        /// </summary>
        [TestMethod]
        public void Evaluations()
        {
            var tests = new[]
            {
                new { startTime = "01/01/2016 00:00:00", inputString = "* * ? * *", nextOccurence = "01/01/2016 00:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2016 00:01:00", inputString = "* * * * ?", nextOccurence = "01/01/2016 00:02:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2016 00:02:00", inputString = "* * ? * ?", nextOccurence = "01/01/2016 00:03:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 00:00:00", inputString = "* * * * *", nextOccurence = "01/01/2003 00:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:01:00", inputString = "* * * * *", nextOccurence = "01/01/2003 00:02:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:02:00", inputString = "* * * * *", nextOccurence = "01/01/2003 00:03:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:58:00", inputString = "* * * * *", nextOccurence = "01/01/2003 00:59:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 01:58:00", inputString = "* * * * *", nextOccurence = "01/01/2003 01:59:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:59:00", inputString = "* * * * *", nextOccurence = "01/01/2003 01:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 01:59:00", inputString = "* * * * *", nextOccurence = "01/01/2003 02:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:59:00", inputString = "* * * * *", nextOccurence = "02/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/12/2003 23:59:00", inputString = "* * * * *", nextOccurence = "01/01/2004 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2003 23:59:00", inputString = "* * * * *", nextOccurence = "01/03/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2004 23:59:00", inputString = "* * * * *", nextOccurence = "29/02/2004 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 00:00:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:00:01", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:01", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:00:02", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:02", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:00:03", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:58", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:00:59", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:01:58", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:01:59", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:59", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:01:00", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:01:59", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:02:00", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 23:59:59", inputString = "* * * * * *", nextOccurence = "02/01/2003 00:00:00", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "31/12/2003 23:59:59", inputString = "* * * * * *", nextOccurence = "01/01/2004 00:00:00", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "28/02/2003 23:59:59", inputString = "* * * * * *", nextOccurence = "01/03/2003 00:00:00", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "28/02/2004 23:59:59", inputString = "* * * * * *", nextOccurence = "29/02/2004 00:00:00", cronStringFormat = CronStringFormat.WithSeconds },

                new { startTime = "01/01/2003 00:00:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:01:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "01/01/2003 00:01:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:02:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "01/01/2003 00:02:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:03:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "01/01/2003 00:58:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 00:59:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "01/01/2003 01:58:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 01:59:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "01/01/2003 00:59:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 01:00:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "01/01/2003 01:59:00", inputString = "* * * * * *", nextOccurence = "01/01/2003 02:00:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "01/01/2003 23:59:00", inputString = "* * * * * *", nextOccurence = "02/01/2003 00:00:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "31/12/2003 23:59:00", inputString = "* * * * * *", nextOccurence = "01/01/2004 00:00:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "28/02/2003 23:59:00", inputString = "* * * * * *", nextOccurence = "01/03/2003 00:00:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "28/02/2004 23:59:00", inputString = "* * * * * *", nextOccurence = "29/02/2004 00:00:00", cronStringFormat = CronStringFormat.WithYears },

                new { startTime = "01/01/2003 00:00:00", inputString = "* * * * * * *", nextOccurence = "01/01/2003 00:00:01", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:01", inputString = "* * * * * * *", nextOccurence = "01/01/2003 00:00:02", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:02", inputString = "* * * * * * *", nextOccurence = "01/01/2003 00:00:03", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:58", inputString = "* * * * * * *", nextOccurence = "01/01/2003 00:00:59", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:01:58", inputString = "* * * * * * *", nextOccurence = "01/01/2003 00:01:59", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:59", inputString = "* * * * * * *", nextOccurence = "01/01/2003 00:01:00", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:01:59", inputString = "* * * * * * *", nextOccurence = "01/01/2003 00:02:00", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 23:59:59", inputString = "* * * * * * *", nextOccurence = "02/01/2003 00:00:00", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "31/12/2003 23:59:59", inputString = "* * * * * * *", nextOccurence = "01/01/2004 00:00:00", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "28/02/2003 23:59:59", inputString = "* * * * * * *", nextOccurence = "01/03/2003 00:00:00", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "28/02/2004 23:59:59", inputString = "* * * * * * *", nextOccurence = "29/02/2004 00:00:00", cronStringFormat = CronStringFormat.WithSecondsAndYears },


                // Second tests

                new { startTime = "01/01/2003 00:00:00", inputString = "45 * * * * *", nextOccurence = "01/01/2003 00:00:45", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:00", inputString = "45 * * * * ?", nextOccurence = "01/01/2003 00:00:45", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:00", inputString = "45 * * ? * *", nextOccurence = "01/01/2003 00:00:45", cronStringFormat = CronStringFormat.WithSeconds },

                new { startTime = "01/01/2003 00:00:00", inputString = "45-47,48,49 * * * * *", nextOccurence = "01/01/2003 00:00:45", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:45", inputString = "45-47,48,49 * * * * *", nextOccurence = "01/01/2003 00:00:46", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:46", inputString = "45-47,48,49 * * * * *", nextOccurence = "01/01/2003 00:00:47", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:47", inputString = "45-47,48,49 * * * * *", nextOccurence = "01/01/2003 00:00:48", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:48", inputString = "45-47,48,49 * * * * *", nextOccurence = "01/01/2003 00:00:49", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:49", inputString = "45-47,48,49 * * * * *", nextOccurence = "01/01/2003 00:01:45", cronStringFormat = CronStringFormat.WithSeconds },

                new { startTime = "01/01/2003 00:00:00", inputString = "2/5 * * * * *", nextOccurence = "01/01/2003 00:00:02", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:02", inputString = "2/5 * * * * *", nextOccurence = "01/01/2003 00:00:07", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:50", inputString = "2/5 * * * * *", nextOccurence = "01/01/2003 00:00:52", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:52", inputString = "2/5 * * * * *", nextOccurence = "01/01/2003 00:00:57", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:00:57", inputString = "2/5 * * * * *", nextOccurence = "01/01/2003 00:01:02", cronStringFormat = CronStringFormat.WithSeconds },

                new { startTime = "01/01/2003 00:00:00", inputString = "45 * * * * * *", nextOccurence = "01/01/2003 00:00:45", cronStringFormat = CronStringFormat.WithSecondsAndYears },

                new { startTime = "01/01/2003 00:00:00", inputString = "45-47,48,49 * * * * * *", nextOccurence = "01/01/2003 00:00:45", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:45", inputString = "45-47,48,49 * * * * * *", nextOccurence = "01/01/2003 00:00:46", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:46", inputString = "45-47,48,49 * * * * * *", nextOccurence = "01/01/2003 00:00:47", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:47", inputString = "45-47,48,49 * * * * * *", nextOccurence = "01/01/2003 00:00:48", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:48", inputString = "45-47,48,49 * * * * * *", nextOccurence = "01/01/2003 00:00:49", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:49", inputString = "45-47,48,49 * * * * * *", nextOccurence = "01/01/2003 00:01:45", cronStringFormat = CronStringFormat.WithSecondsAndYears },

                new { startTime = "01/01/2003 00:00:00", inputString = "2/5 * * * * * *", nextOccurence = "01/01/2003 00:00:02", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:02", inputString = "2/5 * * * * * *", nextOccurence = "01/01/2003 00:00:07", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:50", inputString = "2/5 * * * * * *", nextOccurence = "01/01/2003 00:00:52", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:52", inputString = "2/5 * * * * * *", nextOccurence = "01/01/2003 00:00:57", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                new { startTime = "01/01/2003 00:00:57", inputString = "2/5 * * * * * *", nextOccurence = "01/01/2003 00:01:02", cronStringFormat = CronStringFormat.WithSecondsAndYears },

                // Minute tests

                new { startTime = "01/01/2003 00:00:00", inputString = "45 * * * *", nextOccurence = "01/01/2003 00:45:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 00:00:00", inputString = "45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:45:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:45:00", inputString = "45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:46:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:46:00", inputString = "45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:47:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:47:00", inputString = "45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:48:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:48:00", inputString = "45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:49:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:49:00", inputString = "45-47,48,49 * * * *", nextOccurence = "01/01/2003 01:45:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 00:00:00", inputString = "2/5 * * * *", nextOccurence = "01/01/2003 00:02:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:02:00", inputString = "2/5 * * * *", nextOccurence = "01/01/2003 00:07:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:50:00", inputString = "2/5 * * * *", nextOccurence = "01/01/2003 00:52:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:52:00", inputString = "2/5 * * * *", nextOccurence = "01/01/2003 00:57:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:57:00", inputString = "2/5 * * * *", nextOccurence = "01/01/2003 01:02:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 00:00:30", inputString = "3 45 * * * *", nextOccurence = "01/01/2003 00:45:03", cronStringFormat = CronStringFormat.WithSeconds },

                new { startTime = "01/01/2003 00:00:30", inputString = "6 45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:45:06", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:45:30", inputString = "6 45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:46:06", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:46:30", inputString = "6 45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:47:06", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:47:30", inputString = "6 45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:48:06", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:48:30", inputString = "6 45-47,48,49 * * * *", nextOccurence = "01/01/2003 00:49:06", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:49:30", inputString = "6 45-47,48,49 * * * *", nextOccurence = "01/01/2003 01:45:06", cronStringFormat = CronStringFormat.WithSeconds },

                new { startTime = "01/01/2003 00:00:30", inputString = "9 2/5 * * * *", nextOccurence = "01/01/2003 00:02:09", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:02:30", inputString = "9 2/5 * * * *", nextOccurence = "01/01/2003 00:07:09", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:50:30", inputString = "9 2/5 * * * *", nextOccurence = "01/01/2003 00:52:09", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:52:30", inputString = "9 2/5 * * * *", nextOccurence = "01/01/2003 00:57:09", cronStringFormat = CronStringFormat.WithSeconds },
                new { startTime = "01/01/2003 00:57:30", inputString = "9 2/5 * * * *", nextOccurence = "01/01/2003 01:02:09", cronStringFormat = CronStringFormat.WithSeconds },

                // Hour tests

                new { startTime = "20/12/2003 10:00:00", inputString = " * 3/4 * * *", nextOccurence = "20/12/2003 11:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "20/12/2003 00:30:00", inputString = " * 3   * * *", nextOccurence = "20/12/2003 03:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "20/12/2003 01:45:00", inputString = "30 3   * * *", nextOccurence = "20/12/2003 03:30:00", cronStringFormat = CronStringFormat.Default },

                // Day of month tests

                new { startTime = "07/01/2003 00:00:00", inputString = "30  *  1 * *", nextOccurence = "01/02/2003 00:30:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2003 00:30:00", inputString = "30  *  1 * *", nextOccurence = "01/02/2003 01:30:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 00:00:00", inputString = "10  * 22    * *", nextOccurence = "22/01/2003 00:10:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:00:00", inputString = "30 23 19    * *", nextOccurence = "19/01/2003 23:30:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:00:00", inputString = "30 23 21    * *", nextOccurence = "21/01/2003 23:30:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:01:00", inputString = " *  * 21    * *", nextOccurence = "21/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "10/07/2003 00:00:00", inputString = " *  * 30,31 * *", nextOccurence = "30/07/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "20/01/2016 00:00:00", inputString = " *  * 1W * *", nextOccurence = "01/02/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/04/2016 00:00:00", inputString = " *  * 1W * *", nextOccurence = "02/05/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/09/2016 00:00:00", inputString = " *  * 1W * *", nextOccurence = "03/10/2016 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2003 00:00:00", inputString = " *  * 15W * *", nextOccurence = "14/02/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/06/2003 00:00:00", inputString = " *  * 15W * *", nextOccurence = "16/06/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "10/08/2003 00:00:00", inputString = " *  * LW * *", nextOccurence = "29/08/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "10/10/2015 00:00:00", inputString = " *  * LW * *", nextOccurence = "30/10/2015 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "10/07/2003 00:00:00", inputString = " *  * L * *", nextOccurence = "31/07/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2015 00:00:00", inputString = " *  * L * *", nextOccurence = "28/02/2015 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/02/2016 00:00:00", inputString = " *  * L * *", nextOccurence = "29/02/2016 00:00:00", cronStringFormat = CronStringFormat.Default },

                // Test month rollovers for months with 28,29,30 and 31 days

                new { startTime = "28/02/2002 23:59:59", inputString = "* * * 3 *", nextOccurence = "01/03/2002 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2004 23:59:59", inputString = "* * * 3 *", nextOccurence = "01/03/2004 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/03/2002 23:59:59", inputString = "* * * 4 *", nextOccurence = "01/04/2002 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/04/2002 23:59:59", inputString = "* * * 5 *", nextOccurence = "01/05/2002 00:00:00", cronStringFormat = CronStringFormat.Default },

                // Test month 30,31 days

                new { startTime = "01/01/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/01/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "15/01/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/01/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/01/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "31/01/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/01/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/02/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/02/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/03/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/03/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/03/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/03/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "31/03/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/03/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/04/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/04/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/04/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/04/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/05/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/05/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/05/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/05/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "31/05/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/05/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/06/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/06/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/06/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/06/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/07/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/07/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/07/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/07/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "31/07/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/07/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/08/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/08/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/08/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/08/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "31/08/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/08/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/09/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/09/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/09/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/09/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/10/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/10/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/10/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/10/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "31/10/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/10/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/11/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/11/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/11/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/11/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/12/2000 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "15/12/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "30/12/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/12/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "31/12/2000 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/12/2000 00:00:00", inputString = "0 0 15,30,31 * *", nextOccurence = "15/01/2001 00:00:00", cronStringFormat = CronStringFormat.Default },

                // Other month tests (including year rollover)

                new { startTime = "01/12/2003 05:00:00", inputString = "10 * * 6 *", nextOccurence = "01/06/2004 00:10:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "04/01/2003 00:00:00", inputString = " 1 2 3 * *", nextOccurence = "03/02/2003 02:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/07/2002 05:00:00", inputString = "10 * * February,April-Jun *", nextOccurence = "01/02/2003 00:10:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:00:00", inputString = "0 12 1 6 *", nextOccurence = "01/06/2003 12:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "11/09/1988 14:23:00", inputString = "* 12 1 6 *", nextOccurence = "01/06/1989 12:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "11/03/1988 14:23:00", inputString = "* 12 1 6 *", nextOccurence = "01/06/1988 12:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "11/03/1988 14:23:00", inputString = "* 2,4-8,15 * 6 *", nextOccurence = "01/06/1988 02:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "11/03/1988 14:23:00", inputString = "20 * * january,FeB,Mar,april,May,JuNE,July,Augu,SEPT-October,Nov,DECEM *", nextOccurence = "11/03/1988 15:20:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "11/09/1988 14:23:00", inputString = "* 12 1 6 * 1988,1989", nextOccurence = "01/06/1989 12:00:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "11/09/1988 14:23:00", inputString = "* 12 1 6 * 1988,2000", nextOccurence = "01/06/2000 12:00:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "11/09/1988 14:23:00", inputString = "* 12 1 6 * 1988/5", nextOccurence = "01/06/1993 12:00:00", cronStringFormat = CronStringFormat.WithYears },

                // Day of week tests

                new { startTime = "26/06/2003 10:00:00", inputString = "30 6 * * 0", nextOccurence = "29/06/2003 06:30:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "26/06/2003 10:00:00", inputString = "30 6 * * sunday", nextOccurence = "29/06/2003 06:30:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "26/06/2003 10:00:00", inputString = "30 6 * * SUNDAY", nextOccurence = "29/06/2003 06:30:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "19/06/2003 00:00:00", inputString = "1 12 * * 2", nextOccurence = "24/06/2003 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "24/06/2003 12:01:00", inputString = "1 12 * * 2", nextOccurence = "01/07/2003 12:01:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/06/2003 14:55:00", inputString = "15 18 * * Mon", nextOccurence = "02/06/2003 18:15:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "02/06/2003 18:15:00", inputString = "15 18 * * Mon", nextOccurence = "09/06/2003 18:15:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "09/06/2003 18:15:00", inputString = "15 18 * * Mon", nextOccurence = "16/06/2003 18:15:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "16/06/2003 18:15:00", inputString = "15 18 * * Mon", nextOccurence = "23/06/2003 18:15:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "23/06/2003 18:15:00", inputString = "15 18 * * Mon", nextOccurence = "30/06/2003 18:15:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "30/06/2003 18:15:00", inputString = "15 18 * * Mon", nextOccurence = "07/07/2003 18:15:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 00:00:00", inputString = "* * * * Mon", nextOccurence = "06/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 12:00:00", inputString = "45 16 1 * Mon", nextOccurence = "01/09/2003 16:45:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/09/2003 23:45:00", inputString = "45 16 1 * Mon", nextOccurence = "01/12/2003 16:45:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/09/2003 23:45:00", inputString = "45 16 * * Mon#2", nextOccurence = "08/09/2003 16:45:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/09/2003 23:45:00", inputString = "45 16 * * 2#4", nextOccurence = "23/09/2003 16:45:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * 0L", nextOccurence = "26/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * SUNL", nextOccurence = "26/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * SUNDL", nextOccurence = "26/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * SUNDAYL", nextOccurence = "26/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * 6L", nextOccurence = "25/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * SATL", nextOccurence = "25/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * SATUL", nextOccurence = "25/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:45:00", inputString = "0 0 * * SATURDAYL", nextOccurence = "25/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },

                // Leap year tests

                new { startTime = "01/01/2000 12:00:00", inputString = "1 12 29 2 *", nextOccurence = "29/02/2000 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2000 12:01:00", inputString = "1 12 29 2 *", nextOccurence = "29/02/2004 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2004 12:01:00", inputString = "1 12 29 2 *", nextOccurence = "29/02/2008 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2004 12:01:00", inputString = "1 12 29 2 * */3", nextOccurence = "29/02/2016 12:01:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "29/02/2004 12:01:00", inputString = "1 12 29 2 * 2005/3", nextOccurence = "29/02/2008 12:01:00", cronStringFormat = CronStringFormat.WithYears },
                new { startTime = "29/02/2004 12:01:00", inputString = "1 12 29 2 * 2002/7", nextOccurence = "29/02/2016 12:01:00", cronStringFormat = CronStringFormat.WithYears },

                // Non-leap year tests

                new { startTime = "01/01/2000 12:00:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2000 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2000 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2001 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2001 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2002 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2002 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2003 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2003 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2004 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2004 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2005 12:01:00", cronStringFormat = CronStringFormat.Default },

                // ? filter  tests
            };

            foreach (var test in tests)
                CronCall(test.startTime, test.inputString, test.nextOccurence, test.cronStringFormat);
        }

        [TestMethod]
        public void EvaluationsBlank()
        {
            var tests = new[] {
                // Fire at 12pm (noon) every day
                 new { startTime = "22/05/1983 00:00:00", inputString = "0 0 12 * * ?"   , nextOccurence = "22/05/1983 12:00:00", cronStringFormat = CronStringFormat.WithSeconds },
                 
                 // Fire at 10:15am every day
                 new { startTime = "22/05/1983 00:00:00", inputString = "0 15 10 ? * *"  , nextOccurence = "22/05/1983 10:15:00", cronStringFormat = CronStringFormat.WithSeconds },
                 new { startTime = "22/05/1983 00:00:00", inputString = "0 15 10 * * ?"  , nextOccurence = "22/05/1983 10:15:00", cronStringFormat = CronStringFormat.WithSeconds },
                 new { startTime = "22/05/1983 00:00:00", inputString = "0 15 10 * * ? *", nextOccurence = "22/05/1983 10:15:00", cronStringFormat = CronStringFormat.WithSecondsAndYears },
                 
                 //Fire at 2:10pm and at 2:44pm every Wednesday in the month of March.
                 new { startTime = "22/05/1983 00:00:00", inputString = "0 10,44 14 ? 3 WED", nextOccurence = "07/03/1984 14:10:00", cronStringFormat = CronStringFormat.WithSeconds },
                 
                 // Fire at 10:15 AM on the last day of every month
                 new { startTime = "22/05/1983 00:00:00", inputString = "0 15 10 L * ?", nextOccurence = "31/05/1983 10:15:00", cronStringFormat = CronStringFormat.WithSeconds },
                 
                 // Fire at 10:15 AM on the last Friday of every month
                 new { startTime = "01/07/1984 00:00:00", inputString = "0 15 10 ? * 6L", nextOccurence = "28/07/1984 10:15:00", cronStringFormat = CronStringFormat.WithSeconds },
                                   	
                //Fire at ... AM on every last friday of every month during the years 2002, 2003, 2004, and 2005
                new { startTime = "01/07/1984 00:00:00", inputString = "39 26 13 ? * 5L 2002-2005", nextOccurence = "25/01/2002 13:26:39", cronStringFormat = CronStringFormat.WithSecondsAndYears },

                //Fire at .. AM on the third SATURDAY of every month
                new { startTime = "01/06/2016 00:00:00", inputString = "1 16 11 ? * 6#3", nextOccurence = "18/06/2016 11:16:01", cronStringFormat = CronStringFormat.WithSeconds },

             	//Fire at 12 PM (noon) every 5 days every month, starting on the first day of the month
                new { startTime = "01/07/1984 00:00:00", inputString = "1 2 12 1/5 * ?", nextOccurence = "01/07/1984 12:02:01", cronStringFormat = CronStringFormat.WithSeconds },

                //Fire every November 11 at 11:11 AM
                new { startTime = "01/07/1984 00:00:00", inputString = "0 11 11 11 11 ?", nextOccurence = "11/11/1984 11:11:00", cronStringFormat = CronStringFormat.WithSeconds },
            };
                
            foreach (var test in tests)
                CronCall(test.startTime, test.inputString, test.nextOccurence, test.cronStringFormat);
        }


        [TestMethod]
        public void FiniteOccurrences()
        {
            var tests = new []
            {
                new { inputString = " *  * * * *  ", startTime = "01/01/2003 00:00:00", endTime = "01/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { inputString = " *  * * * *  ", startTime = "31/12/2002 23:59:59", endTime = "01/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { inputString = " *  * * * Mon", startTime = "31/12/2002 23:59:59", endTime = "01/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { inputString = " *  * * * Mon", startTime = "01/01/2003 00:00:00", endTime = "02/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { inputString = " *  * * * Mon", startTime = "01/01/2003 00:00:00", endTime = "02/01/2003 12:00:00", cronStringFormat = CronStringFormat.Default },
                new { inputString = "30 12 * * Mon", startTime = "01/01/2003 00:00:00", endTime = "06/01/2003 12:00:00", cronStringFormat = CronStringFormat.Default },

                new { inputString = " *  *  * * * *  ", startTime = "01/01/2003 00:00:00", endTime = "01/01/2003 00:00:00", cronStringFormat = CronStringFormat.WithSeconds  },
                new { inputString = " *  *  * * * *  ", startTime = "31/12/2002 23:59:59", endTime = "01/01/2003 00:00:00", cronStringFormat = CronStringFormat.WithSeconds  },
                new { inputString = " *  *  * * * Mon", startTime = "31/12/2002 23:59:59", endTime = "01/01/2003 00:00:00", cronStringFormat = CronStringFormat.WithSeconds  },
                new { inputString = " *  *  * * * Mon", startTime = "01/01/2003 00:00:00", endTime = "02/01/2003 00:00:00", cronStringFormat = CronStringFormat.WithSeconds  },
                new { inputString = " *  *  * * * Mon", startTime = "01/01/2003 00:00:00", endTime = "02/01/2003 12:00:00", cronStringFormat = CronStringFormat.WithSeconds  },
                new { inputString = "10 30 12 * * Mon", startTime = "01/01/2003 00:00:00", endTime = "06/01/2003 12:00:10", cronStringFormat = CronStringFormat.WithSeconds  },
            };

            foreach (var test in tests)
                CronFinite(test.inputString, test.startTime, test.endTime, test.cronStringFormat);
        }

        //
        // Test to check we don't loop indefinitely looking for a February
        // 31st because no such date would ever exist!
        //

        [TestMethod]
        public void IllegalDates()
        {
            BadField("* * 0 Feb *", CronStringFormat.Default);
            BadField("* * 31 0 *", CronStringFormat.Default);

            BadField("* * 31 Feb *", CronStringFormat.Default);
            BadField("* * * 31 Feb *", CronStringFormat.WithSeconds);
            BadField("* * 31 Feb * *", CronStringFormat.WithYears);
            BadField("* * * 31 Feb * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * 30-31 Feb *", CronStringFormat.Default);
        }

        [TestMethod]
        static void BadField(string expression, CronStringFormat format)
        {
            Assert2.Throws<CrontabException>(() => CrontabSchedule.Parse(expression, format));
        }

        [TestMethod]
        public void BadSecondsField()
        {
            BadField("bad * * * * *", CronStringFormat.Default);
        }

        [TestMethod]
        public void BadMinutesField()
        {
            BadField("bad * * * *", CronStringFormat.Default);
            BadField("* bad * * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void BadHoursField()
        {
            BadField("* bad * * *", CronStringFormat.Default);
            BadField("* * bad * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void BadDayField()
        {
            BadField("* * bad * *", CronStringFormat.Default);
            BadField("* * * bad * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void BadMonthField()
        {
            BadField("* * * bad *", CronStringFormat.Default);
            BadField("* * * * bad *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void BadDayOfWeekField()
        {
            BadField("* * * * mon,bad,wed", CronStringFormat.Default);
            BadField("* * * * * mon,bad,wed", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void OutOfRangeField()
        {
            BadField("* 1,2,3,456,7,8,9 * * *", CronStringFormat.Default);
            BadField("* * 1,2,3,456,7,8,9 * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void BadYearField()
        {
            BadField("* * * * * Bad", CronStringFormat.WithYears);
            BadField("* * * * * * Bad", CronStringFormat.WithSecondsAndYears);
        }


        [TestMethod]
        public void NonNumberValueInNumericOnlyField()
        {
            BadField("* 1,Z,3,4 * * *", CronStringFormat.Default);
            BadField("* * 1,Z,3,4 * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void BadDayOfWeekStringParsing()
        {
            BadField("Mon * * * * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* Mon * * * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * Mon * * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * * Mon * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * * * Mon * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * * * * * Mon", CronStringFormat.WithSecondsAndYears);
        }

        [TestMethod]
        public void BadMonthStringParsing()
        {
            BadField("Oct * * * * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* Oct * * * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * Oct * * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * * Oct * * *", CronStringFormat.WithSecondsAndYears);
            BadField("* * * * * Oct *", CronStringFormat.WithSecondsAndYears);
            BadField("* * * * * * Oct", CronStringFormat.WithSecondsAndYears);
        }

        [TestMethod]
        public void NonNumericFieldInterval()
        {
            BadField("* 1/Z * * *", CronStringFormat.Default);
            BadField("* * 1/Z * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void NonNumericFieldRangeComponent()
        {
            BadField("* 3-l2 * * *", CronStringFormat.Default);
            BadField("* * 3-l2 * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void MultipleInstancesTest()
        {
            var input = DateTime.Parse("2015-1-1 00:00:00");
            var cronString = "30 8 17W Jan,February 4 2000-2050";

            var parser = CrontabSchedule.Parse(cronString, CronStringFormat.WithYears);
            var instances = parser.GetNextOccurrences(input, DateTime.MaxValue).ToList();
            Assert.AreEqual(10, instances.Count, "Make sure only 10 instances were generated");

            // Now we'll manually iterate through getting values, and check the 11th and 12th
            // instance to make sure nothing blows up.
            var newInput = input;
            for (var i = 0; i < 10; i++)
                newInput = parser.GetNextOccurrence(newInput);

            Assert.IsTrue((newInput = parser.GetNextOccurrence(newInput)) == DateTime.MaxValue, "Make sure 11th instance is the endDate");
            Assert.IsTrue((newInput = parser.GetNextOccurrence(newInput)) == DateTime.MaxValue, "Make sure 12th instance is the endDate");
        }

        [TestMethod]
        public void NoNextInstanceTest()
        {
            var stopWatch = new Stopwatch();

            var cron = NCrontab.Advanced.CrontabSchedule.Parse("0 0 1 1 * 0001", NCrontab.Advanced.Enumerations.CronStringFormat.WithYears);
            var date = DateTime.Parse("0001-01-01");

            stopWatch.Start();
            var result = cron.GetNextOccurrence(date);
            stopWatch.Stop();

            Assert.AreEqual(DateTime.MaxValue, result, "Next date returned is end date");
            Assert.IsFalse(stopWatch.ElapsedMilliseconds > 250, string.Format("Elapsed time should not exceed 250ms (was {0} ms)", stopWatch.ElapsedMilliseconds));
        }

        static void CronCall(string startTimeString, string cronExpression, string nextTimeString, CronStringFormat format)
        {
            var schedule = CrontabSchedule.Parse(cronExpression, format);
            var next = schedule.GetNextOccurrence(Time(startTimeString));

            var message = string.Format("Occurrence of <{0}> after <{1}>, format <{2}>.", cronExpression, startTimeString, Enum.GetName(typeof(CronStringFormat), format));
            Assert.AreEqual(nextTimeString, TimeString(next), message);
        }

        static void CronFinite(string cronExpression, string startTimeString, string endTimeString, CronStringFormat format)
        {
            var schedule = CrontabSchedule.Parse(cronExpression, format);
            var occurrence = schedule.GetNextOccurrence(Time(startTimeString), Time(endTimeString));

            Assert.AreEqual(endTimeString, TimeString(occurrence),
                "Occurrence of <{0}> after <{1}> did not terminate with <{2}>.",
                cronExpression, startTimeString, endTimeString);
        }

        static string TimeString(DateTime time) => time.ToString(TimeFormat, CultureInfo.InvariantCulture);
        static DateTime Time(string str) => DateTime.ParseExact(str, TimeFormat, CultureInfo.InvariantCulture);
    }
}