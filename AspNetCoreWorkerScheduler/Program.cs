using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace AspNetCoreWorkerScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            //TODO: exit on exception
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "config.json"), true, true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddConfig().AddCronJobs();
                })
                .ConfigureLogging((context, logging) =>
                {
                    // remove EventLog provider from UseWindowsService to add it manually with AddEventLog
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddEventLog(new EventLogSettings
                    {
                        LogName = "AspNetCoreWorkerService",
                        SourceName = "AspNetCoreService"
                    });
                });
    }
}
