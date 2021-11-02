using AspNetCoreWorkerScheduler.Configuration.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Interfaces
{
    public interface IConfigurationChangeListener<T> : IDisposable
    {
        event ConfigurationChangedHandler<T> OnConfigurationChangedAsync;

        T CurrentValue { get; }

        Task<T1> AwaitChangesCompletionAfter<T1>(Func<T1> configurationAction);
    }
}
