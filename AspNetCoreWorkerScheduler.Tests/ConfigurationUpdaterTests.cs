using System;
using Microsoft.Extensions.Options;
using Xunit;
using AspNetCoreWorkerScheduler.Configuration;
using AspNetCoreWorkerScheduler.Configuration.Options;
using NSubstitute;

namespace AspNetCoreWorkerScheduler.Tests
{
    public class ConfigurationUpdaterTests
    {
        private readonly IOptionsMonitor<ConfigurationOptions> _configurationOptionsMonitor = Substitute.For<IOptionsMonitor<ConfigurationOptions>>();

        [Fact]
        public void ConfigurationUpdater_RegisterUpdatePathFromOptions_UsesValueWhenProvided()
        {
            const string FAKE_PATH = "path";
            var configurationUpdater = new ConfigurationUpdater(_configurationOptionsMonitor);
            _configurationOptionsMonitor.CurrentValue.Returns(new ConfigurationOptions { FilePath = FAKE_PATH });

            configurationUpdater.RegisterUpdatePath();

            Assert.Equal(FAKE_PATH, configurationUpdater.UpdatePath);
        }
        
        [Fact]
        public void ConfigurationUpdater_RegisterUpdatePathFromOptions_UsesDefaultValueWhenNotProvided()
        {
            var configurationUpdater = new ConfigurationUpdater(_configurationOptionsMonitor);
            _configurationOptionsMonitor.CurrentValue.Returns(new ConfigurationOptions());

            configurationUpdater.RegisterUpdatePath();

            Assert.Equal(default, configurationUpdater.UpdatePath);
        }
        
        [Fact]
        public void ConfigurationUpdater_RegisterUpdatePathManually_AsProvided()
        {
            const string FAKE_PATH = "manual path";
            var configurationUpdater = new ConfigurationUpdater(_configurationOptionsMonitor);

            configurationUpdater.RegisterUpdatePath(FAKE_PATH);

            Assert.Equal(FAKE_PATH, configurationUpdater.UpdatePath);
        }
    }
}
