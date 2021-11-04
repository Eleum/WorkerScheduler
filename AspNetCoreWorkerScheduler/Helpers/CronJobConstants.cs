using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreWorkerScheduler.Exceptions;
using Cronos;
using Microsoft.Extensions.Options;

namespace AspNetCoreWorkerScheduler.Helpers
{
    internal static class CronJobConstants
    {
        public const string NO_DEFAULT_MESSAGE_FOUND = "Default exception message was not found. Type of exception: {0}";

        public static readonly IReadOnlyDictionary<Type, string> DefaultExceptionMessages = new Dictionary<Type, string>()
        {
            { typeof(OptionsValidationException), "Options validation failure occured for {0}:\n{1}" },
            { typeof(CronFormatException), "Invalid cron expression '{0}' for job {1}" },
            { typeof(InvalidNextCronOccurenceException), "Job {0} was not scheduled for execution. Parsed cron expression: {1}, from offset: {2}, timezone: {3}" },
            { typeof(OperationCanceledException), "Job {0} was cancelled" },
            { typeof(Exception), "Unhandled exception occured in {0}: {1}" },
        };

        public static string FormatExceptionMessage<T>(params object[] formatParams) where T: Exception
        {
            var exceptionType = typeof(T);

            return DefaultExceptionMessages.TryGetValue(exceptionType, out var defaultMessage) 
                ? string.Format(defaultMessage, formatParams) 
                : string.Format(NO_DEFAULT_MESSAGE_FOUND, exceptionType);
        }
    }
}
