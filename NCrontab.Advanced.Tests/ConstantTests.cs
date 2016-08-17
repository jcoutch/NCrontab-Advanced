using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCrontab.Advanced.Enumerations;

namespace NCrontab.Advanced.Tests
{
    // Why do we test constants?  To ensure dictionaries that
    // use them are updated as soon as a new value is added!
    [TestClass]
    public class ConstantTests
    {
        [TestMethod]
        public void VerifyConstants()
        {
            ValidateExists<CronStringFormat>(Constants.ExpectedFieldCounts);
            ValidateExists<CrontabFieldKind>(Constants.MaximumDateTimeValues);
            ValidateExists<DayOfWeek>(Constants.CronDays);
        }

        private static void ValidateExists<T>(IDictionary dictionary)
        {
            Assert.IsNotNull(dictionary);

            foreach (var value in Enum.GetValues(typeof (T)))
                Assert.IsTrue(dictionary.Contains(value), "Contains <{0}>", Enum.GetName(typeof(T), value));
        }
    }
}
