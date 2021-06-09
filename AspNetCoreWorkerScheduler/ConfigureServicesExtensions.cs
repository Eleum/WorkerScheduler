using AspNetCoreWorkerScheduler.Interfaces;
using AspNetCoreWorkerScheduler.Jobs;
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
