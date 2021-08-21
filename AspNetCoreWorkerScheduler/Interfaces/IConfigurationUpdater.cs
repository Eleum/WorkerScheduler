using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Interfaces
{
    public interface IConfigurationUpdater
    {
        string UpdatePath { get; }

        void RegisterUpdatePath(string path);

        int AddOrUpdate<T>(string section, T value);
    }
}
