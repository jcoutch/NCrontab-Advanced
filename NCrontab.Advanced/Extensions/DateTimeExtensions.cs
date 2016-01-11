using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCrontab.Advanced.Enumerations;

namespace NCrontab.Advanced.Extensions
{
    class DateTimeExtensions
    {
        public static readonly Dictionary<CrontabFieldKind, int> MaximumValues = new Dictionary<CrontabFieldKind, int>
        {
            { CrontabFieldKind.Second, 60 },
            { CrontabFieldKind.Minute, 60 },
            { CrontabFieldKind.Hour, 24 },
            { CrontabFieldKind.DayOfWeek, 6 },
            { CrontabFieldKind.Day, 31 },
            { CrontabFieldKind.Month, 12 },
            { CrontabFieldKind.Year, 9999 },
        };
    }
}
