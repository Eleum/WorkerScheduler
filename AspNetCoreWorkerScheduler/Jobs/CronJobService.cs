using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private Timer _timer;
        private CancellationTokenSource _currentCts;
        private readonly CronExpression _cronExpression;
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly ILogger _logger;

        public CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, ILogger logger)
        {
            _cronExpression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            _timeZoneInfo = timeZoneInfo;
            _logger = logger;
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            _currentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ScheduleJob(_currentCts.Token);
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _cronExpression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds <= 0)
                {
                    await ScheduleJob(cancellationToken);
                    await Task.CompletedTask;
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
                            await ScheduleJob(cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException e)
                    {
                        _logger.LogWarning($"{this} cron job was cancelled");
                        await StopAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Unhandled exception occured in {this};\n{e.Message}\n{e.StackTrace}");
                        _currentCts.Cancel();

                        await RestartAsync();
                    }
                };
                _timer.Start();
            }

            await Task.CompletedTask;
        }

        public virtual async Task DoWork(CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);
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
            await Task.Delay(7000);
            await StartAsync(cts.Token);
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
            _currentCts?.Cancel();
        }
    }
}
