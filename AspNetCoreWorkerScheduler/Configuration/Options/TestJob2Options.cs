﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Configuration.Options
{
    public class TestJob2Options
    {
        public const string TestJob2 = "TestJob2";

        public string Cron { get; set; }
    }
}
