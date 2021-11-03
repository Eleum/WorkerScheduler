using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using AspNetCoreWorkerScheduler.Configuration;
using AspNetCoreWorkerScheduler.Interfaces;
using AspNetCoreWorkerScheduler.Jobs;

namespace AspNetCoreWorkerScheduler.Tests
{
    public class CronJobServiceTests
    {
        private readonly IConfigurationChangeListener<JobConfiguration> _configurationChangeListener = Substitute.For<IConfigurationChangeListener<JobConfiguration>>();
        private readonly IConfigurationUpdater _configurationUpdater = Substitute.For<IConfigurationUpdater>();
        private readonly ILogger _logger = Substitute.For<ILogger>();
        private readonly JobConfiguration _configuration = Substitute.For<JobConfiguration>();

        [Fact]
        public async Task CronJobService_StartingJob_FinishesWhenInvalidCronSupplied()
        {
            var cronJobService = Substitute.For<CronJobService<JobConfiguration>>(_configurationChangeListener, _configurationUpdater, _logger);
            cronJobService.When(x => x.StartAsync(Arg.Any<CancellationToken>())).CallBase();
            _configuration.Cron = "this is not a cron expression";
            _configurationChangeListener.CurrentValue.Returns(_configuration);

            await cronJobService.StartAsync(CancellationToken.None);

            await cronJobService.Received(1).StopAsync(cronJobService.CancellationToken);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData("          ")]
        public async Task CronJobService_StartingJob_FinishesWhenEmptyOrNoCronSupplied(string cronExpression)
        {
            var cronJobService = Substitute.For<CronJobService<JobConfiguration>>(_configurationChangeListener, _configurationUpdater, _logger);
            cronJobService.When(x => x.StartAsync(Arg.Any<CancellationToken>())).CallBase();
            _configuration.Cron = cronExpression;
            _configurationChangeListener.CurrentValue.Returns(_configuration);

            await cronJobService.StartAsync(CancellationToken.None);

            await cronJobService.Received(1).StopAsync(cronJobService.CancellationToken);
        }
    }
}
