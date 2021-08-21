using AspNetCoreWorkerScheduler.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Configuration.Options
{
    public class TestJob2Options : JobConfiguration
    {
        public const string TestJob2 = nameof(MyTestJob2);
    }
}
