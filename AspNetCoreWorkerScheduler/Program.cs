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
            //�����, ���� ������
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddConfig().AddCronJobs();
                })
                .ConfigureLogging((context, logging) =>
                {
                    // ������� ���������� (��������� EventLog-� �� UseWindowsService) ��� ����������� ���������� ������ ������� � EventViewer
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
