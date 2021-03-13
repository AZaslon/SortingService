using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JobsWebApiService.Commands;
using SortingWebApi.Commands;
using SortingWebApi.Models;


namespace JobsWebApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SortingController : ControllerBase
    {
        private const string AscSortJobType = "sorting_asc";
        private readonly ILogger<SortingController> _logger;
        private readonly IScheduleJobCommandHandler _commandHandler;

        public SortingController(ILogger<SortingController> logger, IScheduleJobCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPost("SortAsync")]
        public async Task<ScheduleJobResponse> SortAsync([FromBody] ScheduleJobRequest job)
        {
            _logger.LogDebug("Job request received.");

            var command = new ScheduleJobCommand(){ JobType = AscSortJobType, JobPayload = JsonSerializer.Serialize(job.Payload)};

            //TODO: implement proper cancellation handling
            var jobDescriptor = await _commandHandler.HandleCommand(command, CancellationToken.None);

            return new ScheduleJobResponse() { Id = jobDescriptor.Id };
        }

        [HttpGet("{id}")]
        public ScheduleJobResponse Get(string id)
        {
            return new ScheduleJobResponse(){Id = id};
        }
    }

    public class ScheduleJobRequest
    {
        public int[] Payload { get; set; }
    }
}
