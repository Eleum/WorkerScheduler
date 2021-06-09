using AspNetCoreWorkerScheduler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler
{
    public class ScheduleConfig<T> : IScheduleConfig<T>
    {
        public string CronExpression { get; set; }

        public TimeZoneInfo TimeZoneInfo { get; set; }
    }
}
