using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsWebApiService.Commands
{
    public class ScheduleJobCommand
    {
        public string JobPayload { get; set; }
        public string JobType { get; set; }
    }
}
