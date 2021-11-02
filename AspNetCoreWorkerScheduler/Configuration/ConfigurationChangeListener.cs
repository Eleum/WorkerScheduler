using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using AspNetCoreWorkerScheduler.Configuration.Handlers;
using AspNetCoreWorkerScheduler.Interfaces;

namespace AspNetCoreWorkerScheduler.Configuration
{
    public class ConfigurationChangeListener<T> : IConfigurationChangeListener<T>
    {
        private readonly IOptionsMonitor<T> _configurationMonitor;
        private readonly IDisposable _changeListener;
        private TaskCompletionSource _changeCompletion;

        public event ConfigurationChangedHandler<T> OnConfigurationChangedAsync;

        public T CurrentValue => _configurationMonitor.CurrentValue;

        public ConfigurationChangeListener(IOptionsMonitor<T> configurationMonitor)
        {
            _configurationMonitor = configurationMonitor;
            _changeListener = _configurationMonitor.OnChange(ConfigurationChangedHandler);
        }

        public async Task<T1> AwaitChangesCompletionAfter<T1>(Func<T1> configurationAction)
        {
            _changeCompletion = new TaskCompletionSource();

            var result = configurationAction();
            await _changeCompletion.Task;

            return result;
        }

        private void ConfigurationChangedHandler(T configuration)
        {
            OnConfigurationChangedAsync?.Invoke(configuration);

            if (_changeCompletion?.Task.IsCompleted ?? true)
                return;

            _changeCompletion?.SetResult();
        }

        public void Dispose()
        {
            _changeListener?.Dispose();
        }
    }
}
