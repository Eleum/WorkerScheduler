using System;

namespace AspNetCoreWorkerScheduler.Interfaces
{
    public interface IScheduleConfig
    {
        string Cron { get; set; }
    }
}