using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetCoreWorkerScheduler.Configuration.Options;
using AspNetCoreWorkerScheduler.Interfaces;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public class MyTestJob1 : CronJobService<TestJob1Options>
    {
        private readonly ILogger<MyTestJob1> _logger;
        private TestJob1Options _config;

        public MyTestJob1(IConfigurationUpdater configUpdater, IOptionsMonitor<TestJob1Options> om, IServiceProvider serviceProvider, ILogger<MyTestJob1> logger) : 
            base(configUpdater, om, serviceProvider, logger)
        {
            _logger = logger;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await base.DoWorkAsync(cancellationToken);

            _logger.LogInformation($"ANY value: {_config.Any}");
            _logger.LogInformation($"Previous execution time: {_config.PreviousExecutionTime}");

            if (new Random().Next(0, 5) == 1)
                throw new Exception("123 hehehhehe");
        }
    }
}
