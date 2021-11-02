using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using AspNetCoreWorkerScheduler.Configuration.Options;
using AspNetCoreWorkerScheduler.Interfaces;

namespace AspNetCoreWorkerScheduler.Configuration
{
    public class ConfigurationUpdater : IConfigurationUpdater
    {
        private readonly IOptionsMonitor<ConfigurationOptions> _globalConfigOptionsMonitor;
        private readonly ReaderWriterLockSlim _rwl = new();

        private string _updatePath;
        public string UpdatePath => _updatePath;

        public ConfigurationUpdater(IOptionsMonitor<ConfigurationOptions> globalConfigOptionsMonitor)
        {
            _globalConfigOptionsMonitor = globalConfigOptionsMonitor;
        }

        public void RegisterUpdatePath()
        {
            _updatePath = _globalConfigOptionsMonitor.CurrentValue?.FilePath;
        }

        public void RegisterUpdatePath(string path)
        {
            _updatePath = path;
        }

        public int AddOrUpdate<T>(string section, T value)
        {
            if (string.IsNullOrWhiteSpace(UpdatePath))
                throw new InvalidOperationException("Update path is not registered");

            var filePath = Path.Combine(AppContext.BaseDirectory, UpdatePath);
            dynamic jsonObj = "";

            _rwl.EnterUpgradeableReadLock();
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    jsonObj = JsonConvert.DeserializeObject(json);
                }

                SetValueRecursively(section, jsonObj, value);
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

                _rwl.EnterWriteLock();
                try
                {
                    File.WriteAllText(filePath, output);
                    return 1;
                }
                finally
                {
                    _rwl.ExitWriteLock();
                }
            }
            catch (Exception e)
            {
                return 0;
            }
            finally
            {
                _rwl.ExitUpgradeableReadLock();
            }
        }

        private static void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                var nextSection = remainingSections[1];
                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                jsonObj[currentSection] = value;
            }
        }
    }
}
