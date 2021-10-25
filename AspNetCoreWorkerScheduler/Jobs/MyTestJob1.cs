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

        public MyTestJob1(
            IConfigurationChangeListener<TestJob1Options> configurationChangeListener,
            IConfigurationUpdater configurationUpdater,
            IServiceProvider serviceProvider,
            ILogger<MyTestJob1> logger) :
            base(configurationChangeListener, configurationUpdater, serviceProvider, logger)
        {
            _logger = logger;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await base.DoWorkAsync(cancellationToken);

            _logger.LogInformation($"ANY value: {Config.Any}");
            _logger.LogInformation($"Previous execution time: {Config.PreviousExecutionTime}");

            if (new Random().Next(0, 5) == 1)
                throw new Exception("123 hehehhehe");
        }
    }
}
