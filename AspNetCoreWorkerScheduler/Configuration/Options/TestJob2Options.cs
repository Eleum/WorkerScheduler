using AspNetCoreWorkerScheduler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Configuration.Options
{
    public class TestJob2Options : IScheduleConfig
    {
        public const string TestJob2 = "TestJob2";

        [Required(AllowEmptyStrings = false)]
        public string Cron { get; set; }
    }
}
