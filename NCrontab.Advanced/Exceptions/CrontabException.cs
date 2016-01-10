using System;

namespace NCrontab.Advanced.Exceptions
{
    public class CrontabException : Exception
    {
        public CrontabException() : base() {}

        public CrontabException(string message) : base(message) {}

        public CrontabException(string message, Exception innerException) : base(message, innerException) {}
    }
}