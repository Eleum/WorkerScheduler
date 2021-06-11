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
        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            services.AddOptions<TestJob1Options>().Bind(configuration.GetSection(TestJob1Options.TestJob1)).ValidateDataAnnotations();
            services.AddOptions<TestJob2Options>().Bind(configuration.GetSection(TestJob2Options.TestJob2)).ValidateDataAnnotations();

            return services;
        }

        public static IServiceCollection AddCronJobs(this IServiceCollection services)
        {
            services.AddHostedService<MyTestJob1>()
                .AddHostedService<MyTestJob2>();

            return services;
        }
    }
}
