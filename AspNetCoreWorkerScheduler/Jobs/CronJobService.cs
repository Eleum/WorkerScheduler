using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetCoreWorkerScheduler.Configuration;
using AspNetCoreWorkerScheduler.Enums;
using AspNetCoreWorkerScheduler.Interfaces;
using AspNetCoreWorkerScheduler.Exceptions;
using AspNetCoreWorkerScheduler.Helpers;
using Timer = System.Timers.Timer;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public abstract class CronJobService<T> : IHostedService, IDisposable where T : JobConfiguration
    {
        private readonly IConfigurationChangeListener<T> _configurationChangeListener;
        private readonly IConfigurationUpdater _configurationUpdater;
        private readonly ILogger _logger;

        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.Local;

        private CancellationTokenSource _cts;
        private CronExpression _cronExpression;
        private Timer _timer;

        public CancellationToken CancellationToken => _cts?.Token ?? CancellationToken.None;

        public T Config
        {
            get
            {
                try
                {
                    return _configurationChangeListener.CurrentValue;
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
            IConfigurationChangeListener<T> configurationChangeListener,
            IConfigurationUpdater configurationUpdater,
            ILogger logger)
        {
            _configurationChangeListener = configurationChangeListener;
            _configurationUpdater = configurationUpdater;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: {this} is starting...");

            try
            {
                await InitializeAsync();
                await ConfigureAsync();
            }
            catch (OptionsValidationException)
            {
                await StopAsync(cancellationToken);
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ScheduleJobAsync(CancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopHandler();

            _configurationChangeListener.OnConfigurationChangedAsync -= ConfigurationReloadHandler;
            _timer?.Stop();

            if (JobStatus != JobStatus.Restarting)
                JobStatus = JobStatus.Stopped;
        }

        public async Task RestartAsync(CancellationToken cancellationToken)
        {
            JobStatus = JobStatus.Restarting;

            await StopAsync(cancellationToken);
            await StartAsync(CancellationToken.None);
        }

        public virtual async Task UpdateConfigurationAsync<T1>(string propertyName, T1 value, CancellationToken cancellationToken)
        {
            await UpdateConfigurationInternal($"{this.GetType().Name}.{propertyName}", value);
        }

        public Task CancelExecutionAsync()
        {
            _cts?.Cancel();
            return Task.CompletedTask;
        }

        private async Task InitializeAsync()
        {
            JobStatus = JobStatus.Initializing;

            await InitializeHandler(Config);

            JobStatus = JobStatus.Initialized;
        }

        private async Task ConfigureAsync(bool allowConfigurationUpdates = true)
        {
            JobStatus = JobStatus.Configuring;

            await ConfigureHandler(allowConfigurationUpdates);

            JobStatus = JobStatus.Configured;
        }

        private async Task ScheduleJobAsync(CancellationToken cancellationToken)
        {
            var from = DateTimeOffset.Now;

            try
            {
                await ScheduleInternal(from, cancellationToken);
                JobStatus = JobStatus.Scheduled;
            }
            catch (InvalidNextCronOccurenceException)
            {
                _logger.LogError(CronJobConstants.FormatDefaultExceptionMessage<InvalidNextCronOccurenceException>(this, Config?.Cron, from, _timeZoneInfo));
                await StopAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(CronJobConstants.FormatDefaultExceptionMessage<OperationCanceledException>(this));
                await StopAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(CronJobConstants.FormatDefaultExceptionMessage<Exception>(this, e.Message?.TrimEnd()));
                await RestartAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Initializes internal cron expression from config
        /// </summary>
        protected virtual Task InitializeHandler(T config)
        {
            _cronExpression = null;

            if (string.IsNullOrWhiteSpace(config?.Cron))
                return Task.CompletedTask;

            _cronExpression = CronExpression.Parse(config.Cron, CronFormat.IncludeSeconds);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Configures job configuration updates if allowed
        /// </summary>
        protected virtual Task ConfigureHandler(bool allowConfigurationUpdates)
        {
            if (allowConfigurationUpdates)
            {
                _configurationUpdater.RegisterUpdatePath();
                _configurationChangeListener.OnConfigurationChangedAsync += ConfigurationReloadHandler;
            }

            return Task.CompletedTask;
        }

        protected virtual Task ExecutionHandler(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: {this} fired execution");
            return Task.CompletedTask;
        }

        protected virtual Task StopHandler()
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: {this} is stopping...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// By default calls <see cref="InitializeAsync"/>
        /// </summary>
        protected virtual Task ConfigurationReloadHandler(T config)
        {
            return InitializeAsync();
        }

        private async Task ScheduleInternal(DateTimeOffset from, CancellationToken cancellationToken)
        {
            var next = _cronExpression?.GetNextOccurrence(from, _timeZoneInfo);
            if (!next.HasValue)
            {
                throw new InvalidNextCronOccurenceException();
            }

            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJobAsync(cancellationToken);
                return;
            }

            _timer = new Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) => await ExecuteInternal(cancellationToken);
            _timer.Start();
        }

        private async Task ExecuteInternal(CancellationToken cancellationToken)
        {
            JobStatus = JobStatus.Executing;

            _timer.Dispose();
            _timer = null;

            if (!cancellationToken.IsCancellationRequested)
                await ExecutionHandler(cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                await ScheduleJobAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task UpdateConfigurationInternal<T1>(string propertyName, T1 value)
        {
            if (propertyName is null)
                throw new ArgumentException(string.Empty, nameof(propertyName));

            await _configurationChangeListener.AwaitChangesCompletionAfter(
                _configurationUpdater, 
                (configurationUpdater) => configurationUpdater.AddOrUpdate(propertyName.Replace('.', ':'), value));
        }

        public virtual void Dispose()
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: {this} is disposing...");

            _configurationChangeListener.Dispose();
            _timer?.Dispose();
            _cts?.Cancel();
        }
    }
}
