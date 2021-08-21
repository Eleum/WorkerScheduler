using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler.Configuration.Options
{
    public class ConfigurationOptions
    {
        public const string SectionPath = nameof(ConfigurationOptions);

        public string FilePath { get; set; }
    }
}
