using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetCoreWorkerScheduler.Configuration.Options;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public class MyTestJob2 : CronJobService<TestJob2Options>
    {
        private readonly ILogger<MyTestJob2> _logger;
        private TestJob2Options _config;

        public MyTestJob2(IServiceProvider serviceProvider, ILogger<MyTestJob2> logger) : base(serviceProvider, logger)
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
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}: Cron job 2 fired execution");
            await Task.CompletedTask;
        }
    }
}
