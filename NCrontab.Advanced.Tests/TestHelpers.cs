using System;
using System.Globalization;
using System.Threading;

namespace NCrontab.Advanced.Tests
{
    public static class TestHelpers
    {
        // https://stackoverflow.com/questions/32382843/how-can-i-set-the-culture-for-individual-mstest-test-methods
        public static void ExecuteWithCulture(string cultureName, Action action)
        {
            Exception exception = null;

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            thread.CurrentCulture = new CultureInfo(cultureName);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw new Exception("Exception occured running in the culture " + cultureName, exception);
        }
    }
}