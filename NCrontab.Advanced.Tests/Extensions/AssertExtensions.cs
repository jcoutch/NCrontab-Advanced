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
                throw new AssertFailedException(string.Format("Expected exception '{0}', but '{1}' was thrown\n\n{2}.  {3}", typeof(T).Name, e.GetType().Name, e, additionalInfo));
            }

            Assert.Fail(string.Format("Expected exception '{0}', but no exception was thrown.  {1}", typeof(T).Name, additionalInfo));
        }
    }
}
