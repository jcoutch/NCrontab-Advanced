using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCrontab.Advanced.Enumerations
{
    /// <summary>
    /// The cron string format to use during parsing
    /// </summary>
    public enum CronStringFormat
    {
        /// <summary>
        /// Defined as "MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK"
        /// </summary>
        Default = 0,

        /// <summary>
        /// Defined as "MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK YEARS"
        /// </summary>
        WithYears = 1,

        /// <summary>
        /// Defined as "SECONDS MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK"
        /// </summary>
        WithSeconds = 2,

        /// <summary>
        /// Defined as "SECONDS MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK YEARS"
        /// </summary>
        WithSecondsAndYears = 3
    }
}
