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
    public class MyTestJob1 : CronJobService
    {
        private readonly TestJob1Options _config;
        private readonly ILogger<MyTestJob1> _logger;

        public MyTestJob1(IOptions<TestJob1Options> options, ILogger<MyTestJob1> logger) : base(logger)
        {
            try
            {
                _logger = logger;
                _config = options.Value;
            }
            catch (OptionsValidationException e)
            {
                _logger.LogError($"Options validation occured for {this}:\n{string.Join("; ", e.Failures)}");
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: Cron job 1 is starting...");

            await InitializeCoreAsync(_config, cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: Cron job 1 fired execution");

            if (new Random().Next(0, 5) == 1)
                throw new Exception("123 hehehhehe");

            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{DateTime.Now:hh:mm:ss}: Cron job 1 is stopping...");
            return base.StopAsync(cancellationToken);
        }
    }
}
