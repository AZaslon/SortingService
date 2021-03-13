
using System;

namespace JobsWebApiService.Commands
{
    public class JobSchedulingOptions
    {
        public TimeSpan SlidingExpiration { get; set; }
    }
}