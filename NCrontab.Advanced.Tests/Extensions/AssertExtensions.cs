using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCrontab.Advanced.Tests.Extensions
{
    public static class Assert2
    {
        public static void Throws<T>(Action methodToCall, string message = "", params object[] values) where T : Exception
        {
            var additionalInfo = string.Format(message, values);
            try
            {
                methodToCall();
            }
            catch (T)
            {
                return;
            }
            catch (Exception e)
            {
                throw new AssertFailedException($"Expected exception '{typeof(T).Name}', but '{e.GetType().Name}' was thrown\n\n{e}.  {additionalInfo}");
            }

            Assert.Fail($"Expected exception '{typeof(T).Name}', but no exception was thrown.  {additionalInfo}");
        }
    }
}
