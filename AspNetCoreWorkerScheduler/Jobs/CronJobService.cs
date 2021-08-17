using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Cronos;
using AspNetCoreWorkerScheduler.Configuration;
using AspNetCoreWorkerScheduler.Enums;
using Timer = System.Timers.Timer;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public abstract class CronJobService<T> : IHostedService, IDisposable where T: SchedulerConfig
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.Local;

        private CancellationTokenSource _currentCts;
        private CronExpression _cronExpression;
        private Timer _timer;

        public JobStatus JobStatus { get; set; }

        public CronJobService(IServiceProvider serviceProvider, ILogger logger)
        {
            JobStatus = JobStatus.Initializing;

            _serviceProvider = serviceProvider;
            _logger = logger;

            JobStatus = JobStatus.Initialized;
        }

        protected async Task<T> GetCurrentScopeConfig()
        {
            JobStatus = JobStatus.Configuring;
            T config;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                config = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<T>>().Value;
            }
            catch (OptionsValidationException e)
            {
                _logger.LogError($"Options validation occured for {this}:\n\t{string.Join("; ", e.Failures)}");
                await StopAsync(_currentCts is null ? default : _currentCts.Token);

                return default;
            }

            JobStatus = JobStatus.Configured;
            return config;
        }

        protected async Task InitializeCoreAsync(SchedulerConfig config)
        {
            if (string.IsNullOrWhiteSpace(config?.Cron)) return;
            
            _cronExpression = CronExpression.Parse(config.Cron, CronFormat.IncludeSeconds);
            await Task.CompletedTask;
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: {this} is starting...");

            _currentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ScheduleJob(_currentCts.Token);
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
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
                await ScheduleJob(cancellationToken);
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
                        await DoWork(cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                        await UpdateConfig(cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                        await ScheduleJob(cancellationToken);

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

        public virtual async Task DoWork(CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);
        }

        public virtual async Task UpdateConfig(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
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

        public virtual void Dispose()
        {
            _timer?.Dispose();
            _currentCts?.Cancel();
        }
    }
}
