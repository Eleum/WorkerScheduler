using AspNetCoreWorkerScheduler.Configuration.Options;
using AspNetCoreWorkerScheduler.Interfaces;
using AspNetCoreWorkerScheduler.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWorkerScheduler
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection LoadConfig(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            var testJob1Options = configuration.GetSection(TestJob1Options.TestJob1).Get<TestJob1Options>();
            var testJob2Options = configuration.GetSection(TestJob2Options.TestJob2).Get<TestJob2Options>();

            services.AddCronJob<MyTestJob1>(options =>
                {
                    options.TimeZoneInfo = TimeZoneInfo.Local;
                    options.CronExpression = testJob1Options.Cron;
                })
                .AddCronJob<MyTestJob2>(options =>
                {
                    options.TimeZoneInfo = TimeZoneInfo.Local;
                    options.CronExpression = testJob2Options.Cron;
                });

            services.Configure<TestJob1Options>(configuration.GetSection(TestJob1Options.TestJob1));
            services.Configure<TestJob2Options>(configuration.GetSection(TestJob2Options.TestJob2));

            return services;
        }    

        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T: CronJobService
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options), "Необходимо указать конфигурацию шедулера");

            var config = new ScheduleConfig<T>();
            options.Invoke(config);

            if (string.IsNullOrWhiteSpace(config.CronExpression))
                throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), "Пустое Cron-описание задачи недопустимо");

            services.AddSingleton<IScheduleConfig<T>>(config);
            services.AddHostedService<T>();

            return services;
        }
    }
}
