using AspNetCoreWorkerScheduler.Configuration;
using AspNetCoreWorkerScheduler.Interfaces;
using Moq;
using System;
using Xunit;

namespace AspNetCoreWorkerScheduler.Tests
{
    public class ConfigurationUpdaterTests
    {
        [Fact]
        public void ConfigurationUpdater_RegisterUpdatePath_AsProvided()
        {
            var configurationUpdater = new ConfigurationUpdater();

            configurationUpdater.RegisterUpdatePath("path");

            Assert.Equal("path", configurationUpdater.UpdatePath);
        }

        // test CronJobService
    }
}
