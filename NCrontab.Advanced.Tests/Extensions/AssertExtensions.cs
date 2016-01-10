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
        public static void Throws<T>(Action methodToCall) where T : Exception
        {
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
                Assert.Fail(string.Format("Expected exception '{0}', but '{1}' was thrown", nameof(T), e.GetType().Name));
            }

            Assert.Fail(string.Format("Expected exception '{0}', but no exception was thrown", nameof(T)));
        }
    }
}
