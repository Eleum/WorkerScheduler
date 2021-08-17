using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreWorkerScheduler.Configuration
{
    public abstract class SchedulerConfig
    {
        [Required(AllowEmptyStrings = false)]
        public string Cron { get; set; }

        public string Any { get; set; }
    }
}