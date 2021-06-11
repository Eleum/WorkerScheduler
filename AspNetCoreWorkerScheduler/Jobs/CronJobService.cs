using AspNetCoreWorkerScheduler.Interfaces;
using Cronos;
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
using Timer = System.Timers.Timer;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public abstract class CronJobService : IHostedService, IDisposable
    {
        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.Local;
        private readonly ILogger _logger;

        private Timer _timer;
        private CancellationTokenSource _currentCts;
        private CronExpression _cronExpression;

        public CronJobService(ILogger logger)
        {
            _logger = logger;
        }

        protected async Task InitializeCoreAsync(IScheduleConfig config, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(config?.Cron)) return;

            _cronExpression = CronExpression.Parse(config.Cron, CronFormat.IncludeSeconds);
            await Task.CompletedTask;
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            _currentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ScheduleJob(_currentCts.Token);
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _cronExpression?.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            if (!next.HasValue)
            {
                _logger.LogError($"{this} was not scheduled for execution");
                return;
            }

            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJob(cancellationToken);
                return;
            }

            _timer = new Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                try
                {
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
                    _logger.LogError($"Unhandled exception occured in {this}: {e.Message?.TrimEnd()}\nStack trace:\n{e.StackTrace}");
                    await ScheduleJob(cancellationToken);
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
            _timer?.Stop();
            await Task.CompletedTask;
        }

        public virtual async Task RestartAsync()
        {
            var cts = new CancellationTokenSource();

            await StopAsync(_currentCts.Token);
            await StartAsync(cts.Token);
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
            _currentCts?.Cancel();
        }
    }
}
