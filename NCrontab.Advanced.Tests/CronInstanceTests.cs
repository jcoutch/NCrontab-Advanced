#region License and Terms
//
// Most unit tests are from NCrontab - Crontab for .NET, written by Atif Aziz.
// Ported to Microsoft's unit test framework by Joe Coutcher
// Project can be accessed at https://github.com/atifaziz/NCrontab
//
#endregion

using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCrontab.Advanced.Enumerations;
using NCrontab.Advanced.Exceptions;
using NCrontab.Advanced.Parsers;
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
            Assert2.Throws<ArgumentNullException>(() => CronInstance.Parse(null));
        }

        [TestMethod]
        public void CannotParseEmptyString()
        {
            Assert2.Throws<CrontabException>(() => CronInstance.Parse(string.Empty));
        }

        [TestMethod]
        public void AllTimeString()
        {
            var input = "* * * * *";
            var output = CronInstance.Parse(input, CronStringFormat.Default).ToString();
            Assert.AreEqual(input, output);
        }

        [TestMethod]
        public void SixPartAllTimeString()
        {
            Assert.AreEqual("* * * * * *", CronInstance.Parse("* * * * * *", CronStringFormat.WithSeconds).ToString());
        }

        [TestMethod]
        public void CannotParseWhenSecondsRequired()
        {
            Assert2.Throws<CrontabException>(() => CronInstance.Parse("* * * * *", CronStringFormat.WithSeconds));
        }

        [TestMethod]
        public void Formatting()
        {
            var tests = new[] {
                new { inputString = "* 1-3 * * *"            , outputString = "* 1-2,3 * * *"                   , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "* * * 1,3,5,7,9,11 *"   , outputString = "* * * */2 *"                     , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "10,25,40 * * * *"       , outputString = "10-40/15 * * * *"                , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "* * * 1,3,8 1-2,5"      , outputString = "* * * Mar,Jan,Aug Fri,Mon-Tue"   , cronStringFormat = CronStringFormat.Default },
	            new { inputString = "1 * 1-3 * * *"          , outputString = "1 * 1-2,3 * * *"                 , cronStringFormat = CronStringFormat.WithSeconds },
	            new { inputString = "22 * * * 1,3,5,7,9,11 *", outputString = "22 * * * */2 *"                  , cronStringFormat = CronStringFormat.WithSeconds },
	            new { inputString = "33 10,25,40 * * * *"    , outputString = "33 10-40/15 * * * *"             , cronStringFormat = CronStringFormat.WithSeconds },
	            new { inputString = "55 * * * 1,3,8 1-2,5"   , outputString = "55 * * * Mar,Jan,Aug Fri,Mon-Tue", cronStringFormat = CronStringFormat.WithSeconds },
            };

            foreach (var test in tests)
                Assert.AreEqual(test.outputString, CronInstance.Parse(test.inputString, test.cronStringFormat).ToString());
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
                new { startTime = "01/01/2003 00:00:00", inputString = "* * * * *", nextOccurence = "01/01/2003 00:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:01:00", inputString = "* * * * *", nextOccurence = "01/01/2003 00:02:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:02:00", inputString = "* * * * *", nextOccurence = "01/01/2003 00:03:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 00:59:00", inputString = "* * * * *", nextOccurence = "01/01/2003 01:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 01:59:00", inputString = "* * * * *", nextOccurence = "01/01/2003 02:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "01/01/2003 23:59:00", inputString = "* * * * *", nextOccurence = "02/01/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "31/12/2003 23:59:00", inputString = "* * * * *", nextOccurence = "01/01/2004 00:00:00", cronStringFormat = CronStringFormat.Default },

                new { startTime = "28/02/2003 23:59:00", inputString = "* * * * *", nextOccurence = "01/03/2003 00:00:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2004 23:59:00", inputString = "* * * * *", nextOccurence = "29/02/2004 00:00:00", cronStringFormat = CronStringFormat.Default },

                // Second tests

                new { startTime = "01/01/2003 00:00:00", inputString = "45 * * * * *", nextOccurence = "01/01/2003 00:00:45", cronStringFormat = CronStringFormat.WithSeconds },

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

                // Leap year tests

                new { startTime = "01/01/2000 12:00:00", inputString = "1 12 29 2 *", nextOccurence = "29/02/2000 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2000 12:01:00", inputString = "1 12 29 2 *", nextOccurence = "29/02/2004 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2004 12:01:00", inputString = "1 12 29 2 *", nextOccurence = "29/02/2008 12:01:00", cronStringFormat = CronStringFormat.Default },

                // Non-leap year tests

                new { startTime = "01/01/2000 12:00:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2000 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2000 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2001 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2001 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2002 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2002 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2003 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "28/02/2003 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2004 12:01:00", cronStringFormat = CronStringFormat.Default },
                new { startTime = "29/02/2004 12:01:00", inputString = "1 12 28 2 *", nextOccurence = "28/02/2005 12:01:00", cronStringFormat = CronStringFormat.Default },
            };

            foreach (var test in tests)
                CronCall(test.startTime, test.inputString, test.nextOccurence, test.cronStringFormat);
        }

        [TestMethod]
        public void FiniteOccurrences(string cronExpression, string startTimeString, string endTimeString, bool includingSeconds)
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

        [TestCategory("Performance")]
        [Timeout(1000)]
        [TestMethod]
        public void DontLoopIndefinitely()
        {
            CronFinite("* * 31 Feb *", "01/01/2001 00:00:00", "01/01/2010 00:00:00", CronStringFormat.Default);
            CronFinite("* * * 31 Feb *", "01/01/2001 00:00:00", "01/01/2010 00:00:00", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        static void BadField(string expression, CronStringFormat format)
        {
            Assert2.Throws<CrontabException>(() => CronInstance.Parse(expression, format));
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
        public void NonNumberValueInNumericOnlyField()
        {
            BadField("* 1,Z,3,4 * * *", CronStringFormat.Default);
            BadField("* * 1,Z,3,4 * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void NonNumericFieldInterval(string expression, bool includingSeconds)
        {
            BadField("* 1/Z * * *", CronStringFormat.Default);
            BadField("* * 1/Z * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        public void NonNumericFieldRangeComponent(string expression, bool includingSeconds)
        {
            BadField("* 3-l2 * * *", CronStringFormat.Default);
            BadField("* * 3-l2 * * *", CronStringFormat.WithSeconds);
        }

        [TestMethod]
        static void CronCall(string startTimeString, string cronExpression, string nextTimeString, CronStringFormat format)
        {
            var schedule = CronInstance.Parse(cronExpression, format);
            var next = schedule.GetNextOccurrence(Time(startTimeString));

            Assert.AreEqual(nextTimeString, TimeString(next),
                "Occurrence of <{0}> after <{1}>.", cronExpression, startTimeString);
        }

        [TestMethod]
        static void CronFinite(string cronExpression, string startTimeString, string endTimeString, CronStringFormat format)
        {
            var schedule = CronInstance.Parse(cronExpression, format);
            var occurrence = schedule.GetNextOccurrence(Time(startTimeString), Time(endTimeString));

            Assert.AreEqual(endTimeString, TimeString(occurrence),
                "Occurrence of <{0}> after <{1}> did not terminate with <{2}>.",
                cronExpression, startTimeString, endTimeString);
        }

        static string TimeString(DateTime time) => time.ToString(TimeFormat, CultureInfo.InvariantCulture);
        static DateTime Time(string str) => DateTime.ParseExact(str, TimeFormat, CultureInfo.InvariantCulture);
    }
}