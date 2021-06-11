using AspNetCoreWorkerScheduler.Configuration.Options;
using AspNetCoreWorkerScheduler.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly TestJob2Options _config;
        private readonly ILogger<MyTestJob2> _logger;

        public MyTestJob2(IOptions<TestJob2Options> options, ILogger<MyTestJob2> logger) : base(logger)
        {
            try
            {
                _logger = logger;
                _config = options.Value;
            }
            catch (OptionsValidationException e)
            {
                _logger.LogError($"Options validation occured for {this} in {string.Join("; ", e.Failures)}");
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: Cron job 2 is starting...");

            await InitializeCoreAsync(_config, cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: Cron job 2 fired execution");
            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: Cron job 2 is stopping...");
            return base.StopAsync(cancellationToken);
        }
    }
}
