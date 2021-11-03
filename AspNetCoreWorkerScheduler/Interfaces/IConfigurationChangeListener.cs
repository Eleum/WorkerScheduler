using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreWorkerScheduler.Configuration.Handlers;

namespace AspNetCoreWorkerScheduler.Interfaces
{
    public interface IConfigurationChangeListener<T> : IDisposable
    {
        event ConfigurationChangedHandler<T> OnConfigurationChangedAsync;

        T CurrentValue { get; }

        public Task<T1> AwaitChangesCompletionAfter<T1>(IConfigurationUpdater configurationUpdater, Func<IConfigurationUpdater, T1> configurationAction);
    }
}
