using AspNetCoreWorkerScheduler.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public class MyTestJob2 : CronJobService
    {
        private readonly ILogger<MyTestJob2> _logger;

        public MyTestJob2(IScheduleConfig<MyTestJob2> config, ILogger<MyTestJob2> logger) : base(config.CronExpression, config.TimeZoneInfo, logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("cron job 2 started");
            return base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: cron job 2 fired execution");
            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"cron job 2 is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}
