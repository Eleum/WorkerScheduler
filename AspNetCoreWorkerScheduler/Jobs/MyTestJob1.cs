using AspNetCoreWorkerScheduler.Configuration.Options;
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
    public class MyTestJob1 : CronJobService<TestJob1Options>
    {
        private readonly ILogger<MyTestJob1> _logger;
        private TestJob1Options _config;

        public MyTestJob1(IServiceProvider serviceProvider, ILogger<MyTestJob1> logger) : base(serviceProvider, logger)
        {
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _config = await GetCurrentScopeConfig();
            if (_config is null) return;

            await InitializeCoreAsync(_config);
            await base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: Cron job 1 fired execution");
            _logger.LogInformation($"ANY value: {_config.Any}");

            if (new Random().Next(0, 5) == 1)
                throw new Exception("123 hehehhehe");

            await Task.CompletedTask;
        }
    }
}
