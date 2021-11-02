using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Interfaces
{
    public interface IConfigurationUpdater
    {
        /// <summary>
        /// Path to configuration file containing configurations for jobs
        /// </summary>
        string UpdatePath { get; }

        /// <summary>
        /// Register path from <see cref="Configuration.Options.ConfigurationOptions"/> options to the file containing jobs configuration
        /// </summary>
        void RegisterUpdatePath();

        /// <summary>
        /// Manually register path to the file containing jobs configuration 
        /// </summary>
        void RegisterUpdatePath(string path);

        int AddOrUpdate<T>(string section, T value);
    }
}
