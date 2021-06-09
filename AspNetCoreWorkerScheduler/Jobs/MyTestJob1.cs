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
    public class MyTestJob1 : CronJobService
    {
        private readonly ILogger<MyTestJob1> _logger;

        public MyTestJob1(IScheduleConfig<MyTestJob1> config, ILogger<MyTestJob1> logger) : base(config.CronExpression, config.TimeZoneInfo, logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: cron job 1 started");
            return base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: cron job 1 fired execution");

            if (new Random().Next(0, 5) == 1)
                throw new Exception("123 hehehhehe");

            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: cron job 1 is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}
