using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Configuration.Handlers
{
    public delegate Task ConfigurationChangedHandler<T>(T config);
}
