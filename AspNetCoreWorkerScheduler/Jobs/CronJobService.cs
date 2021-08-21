using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetCoreWorkerScheduler.Configuration;
using AspNetCoreWorkerScheduler.Configuration.Options;
using AspNetCoreWorkerScheduler.Enums;
using AspNetCoreWorkerScheduler.Interfaces;
using Timer = System.Timers.Timer;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public abstract class CronJobService<T> : IHostedService, IDisposable where T: JobConfiguration
    {
        private readonly IConfigurationUpdater _configurationUpdater;
        private readonly IOptionsMonitor<T> _configurationMonitor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.Local;

        private CancellationTokenSource _currentCts;
        private CronExpression _cronExpression;
        private Timer _timer;

        public event Func<T, Task> OnConfigurationReloadAsync;

        private IDisposable _configurationChangeListener;
        private TaskCompletionSource _configurationChangeCompleted;

        public T Config
        {
            get
            {
                try
                {
                    return _configurationMonitor.CurrentValue;
                }
                catch (OptionsValidationException e)
                {
                    _logger.LogError($"Options validation failure occured for {this}:\n\t{string.Join("; ", e.Failures)}");
                    throw;
                }
            }
        }

        public JobStatus JobStatus { get; set; }

        public CronJobService(
            IConfigurationUpdater configurationUpdater, 
            IOptionsMonitor<T> configurationMonitor, 
            IServiceProvider serviceProvider, 
            ILogger logger)
        {
            JobStatus = JobStatus.Initializing;

            _configurationUpdater = configurationUpdater;
            _configurationMonitor = configurationMonitor;
            _serviceProvider = serviceProvider;
            _logger = logger;

            JobStatus = JobStatus.Initialized;
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: {this} is starting...");

            try
            {
                await InitializeAsync(Config);
            }
            catch (OptionsValidationException)
            {
                await StopAsync(cancellationToken);
                return;
            }

            await ConfigureAsync();

            _currentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ScheduleJobAsync(_currentCts.Token);
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (JobStatus == JobStatus.Stopped) return;

            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: {this} is stopping...");

            _timer?.Stop();

            if (JobStatus != JobStatus.Restarting)
                JobStatus = JobStatus.Stopped;

            await Task.CompletedTask;
        }

        public virtual async Task RestartAsync(CancellationToken cancellationToken)
        {
            JobStatus = JobStatus.Restarting;
            var cts = new CancellationTokenSource();

            await StopAsync(cancellationToken);
            await StartAsync(cts.Token);
        }

        protected virtual async Task InitializeAsync(JobConfiguration config)
        {
            _cronExpression = null;
            if (string.IsNullOrWhiteSpace(config?.Cron)) return;

            _cronExpression = CronExpression.Parse(config.Cron, CronFormat.IncludeSeconds);
            _logger.LogDebug($"initialized with prev exec time: {config.PreviousExecutionTime}");

            await Task.CompletedTask;
        }

        protected Task ConfigureAsync(bool allowConfigurationUpdates = true)
        {
            JobStatus = JobStatus.Configuring;

            OnConfigurationReloadAsync += ConfigurationReloadHandler;
            _configurationChangeListener = _configurationMonitor.OnChange(ConfigurationChangeHandler);

            if (allowConfigurationUpdates)
            {
                RegisterConfigurationUpdater();
            }

            JobStatus = JobStatus.Configured;

            return Task.CompletedTask;
        }

        protected virtual async Task ScheduleJobAsync(CancellationToken cancellationToken)
        {
            var next = _cronExpression?.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            if (!next.HasValue)
            {
                _logger.LogError($"{this} was not scheduled for execution");

                if (JobStatus == JobStatus.Restarting)
                    JobStatus = JobStatus.Stopped;

                await StopAsync(cancellationToken);
                return;
            }

            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJobAsync(cancellationToken);
                return;
            }

            JobStatus = JobStatus.Scheduled;

            _timer = new Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                try
                {
                    JobStatus = JobStatus.Executing;

                    _timer.Dispose();
                    _timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                        await DoWorkAsync(cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                        await ScheduleJobAsync(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogWarning($"{this} was cancelled");
                    await StopAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unhandled exception occured in {this}: {e.Message?.TrimEnd()}");
                    await RestartAsync(_currentCts.Token);
                }
            };
            _timer.Start();

            await Task.CompletedTask;
        }

        protected virtual async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: {this} fired execution");
            await Task.CompletedTask;
        }

        public virtual async Task UpdateConfigurationAsync(T value, CancellationToken cancellationToken)
        {
            await UpdateConfigurationInternal(this.GetType().Name, value);
        }
        
        public virtual async Task UpdateConfigurationAsync<T1>(string propertyName, T1 value, CancellationToken cancellationToken)
        {
            await UpdateConfigurationInternal($"{this.GetType().Name}.{propertyName}", value);
        }

        private async Task UpdateConfigurationInternal<T1>(string propertyName, T1 value)
        {
            if (propertyName is null)
                throw new ArgumentException(string.Empty, nameof(propertyName));

            _configurationUpdater.AddOrUpdate(propertyName.Replace('.', ':'), value);
            _configurationChangeCompleted = new TaskCompletionSource();
            
            await _configurationChangeCompleted.Task;
        }

        private void RegisterConfigurationUpdater()
        {
            using var scope = _serviceProvider.CreateScope();
            var schedulerConfigOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ConfigurationOptions>>().Value;
            _configurationUpdater.RegisterUpdatePath(schedulerConfigOptions.FilePath);
        }

        private void ConfigurationChangeHandler(T configuration)
        {
            OnConfigurationReloadAsync?.Invoke(configuration);

            if (_configurationChangeCompleted?.Task.IsCompleted ?? true) return;
            _configurationChangeCompleted.SetResult();
        }

        /// <summary>
        /// By default reinitializes cron expression used for service execution
        /// </summary>
        protected virtual async Task ConfigurationReloadHandler(T config)
        {
            await InitializeAsync(config);
        }

        public virtual void Dispose()
        {
            _configurationChangeListener?.Dispose();
            _timer?.Dispose();
            _currentCts?.Cancel();
        }
    }
}
