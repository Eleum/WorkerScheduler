using Cronos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreWorkerScheduler.Helpers;

namespace AspNetCoreWorkerScheduler.Exceptions
{
    public class InvalidNextCronOccurenceException : Exception
    {
        public InvalidNextCronOccurenceException() : base() { }
    }
}
