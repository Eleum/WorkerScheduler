using AspNetCoreWorkerScheduler.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            //выйти, если ошибка
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddCronJob<MyTestJob1>(options =>
                    {
                        options.TimeZoneInfo = TimeZoneInfo.Local;
                        options.CronExpression = @"*/1 * * * * *";
                    });
                    services.AddCronJob<MyTestJob2>(options =>
                    {
                        options.TimeZoneInfo = TimeZoneInfo.Local;
                        options.CronExpression = @"*/5 * * * * *";
                    });
                })
                .ConfigureLogging((context, logging) =>
                {
                    // clear providers to allow AddEventLog() create a new journal in EventViewer
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddEventLog(new EventLogSettings
                    {
                        SourceName = "TestSource",
                        LogName = "AspNetCoreWorkerService"
                    });
                })
                .UseWindowsService();
    }
}
