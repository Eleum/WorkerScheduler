using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreWorkerScheduler.Configuration
{
    public abstract class JobConfiguration
    {
        [Required(AllowEmptyStrings = false)]
        public string Cron { get; set; }

        public DateTime PreviousExecutionTime { get; set; }

        public string Any { get; set; }
    }
}