using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Enums
{
    public enum JobStatus
    {
        Initializing,
        Scheduled,
        Executing,
        Stopped,
        Restarting
    }
}
