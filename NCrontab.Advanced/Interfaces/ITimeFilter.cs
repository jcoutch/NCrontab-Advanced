using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCrontab.Advanced.Interfaces
{
    interface ITimeFilter
    {
        int? Next(int value);
        int First();
    }
}
