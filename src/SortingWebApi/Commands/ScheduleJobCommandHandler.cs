using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JobsWebApiService.Commands;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SortingWebApi.JobsScheduler;

namespace SortingWebApi.Commands
{
    /// <summary>
    /// This handler implements logic for scheduling of new jobDescriptor for processing.
    /// </summary>
    public class ScheduleJobCommandHandler: IScheduleJobCommandHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IJobsQueue _jobsQueue;
        private readonly ILogger<ScheduleJobCommandHandler> _logger;


        public ScheduleJobCommandHandler(IDistributedCache cache, IJobsQueue jobsQueue, ILogger<ScheduleJobCommandHandler>  logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache) + " cannot be null.");
            _jobsQueue = jobsQueue;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<JobDescriptor> HandleCommand(ScheduleJobCommand command, CancellationToken cancellationToken)
        {

            try
            {
                // Create jobDescriptor
                var job = new JobDescriptor(Guid.NewGuid().ToString(), command.JobType, DateTime.UtcNow,
                    command.JobPayload, new JobSchedulingOptions() {SlidingExpiration = TimeSpan.FromMinutes(1)});

                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));

                // Place jobDescriptor into Cache
                await _cache.SetAsync(job.Id, bytes,
                    new DistributedCacheEntryOptions() {SlidingExpiration = job.JobSchedulingOptions.SlidingExpiration},
                    cancellationToken);

                await _jobsQueue.Schedule(job, cancellationToken);

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to HandleCommand: " + JsonSerializer.Serialize(command), ex);
                throw;
            }
        }
    }
}
