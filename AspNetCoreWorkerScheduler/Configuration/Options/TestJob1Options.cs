using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Configuration.Options
{
    public class TestJob1Options
    {
        public const string TestJob1 = "TestJob1";

        public string Cron { get; set; }
    }
}
