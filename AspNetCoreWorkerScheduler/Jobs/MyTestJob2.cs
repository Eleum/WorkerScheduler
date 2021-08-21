using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetCoreWorkerScheduler.Configuration.Options;
using AspNetCoreWorkerScheduler.Interfaces;

namespace AspNetCoreWorkerScheduler.Jobs
{
    public class MyTestJob2 : CronJobService<TestJob2Options>
    {
        private readonly ILogger<MyTestJob2> _logger;

        public MyTestJob2(
            IConfigurationUpdater configurationUpdater, 
            IOptionsMonitor<TestJob2Options> configurationMonitor, 
            IServiceProvider serviceProvider, 
            ILogger<MyTestJob2> logger) :
            base(configurationUpdater, configurationMonitor, serviceProvider, logger)
        {
            _logger = logger;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await base.DoWorkAsync(cancellationToken);

            _logger.LogInformation($"\n\tANY value: {Config.Any}\n\tPrevious execution time: {Config.PreviousExecutionTime:yyyy-MM-dd HH:mm:ss}");

            await UpdateConfigurationAsync(nameof(TestJob2Options.PreviousExecutionTime), DateTime.Now, cancellationToken);
        }
    }
}
