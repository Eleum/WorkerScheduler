﻿using AspNetCoreWorkerScheduler.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Configuration.Options
{
    public class TestJob1Options : JobConfiguration
    {
        public const string TestJob1 = nameof(MyTestJob1);
    }
}
