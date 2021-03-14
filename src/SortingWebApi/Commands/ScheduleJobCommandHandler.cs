using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JobsWebApiService.Commands;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SortingWebApi.Common;
using SortingWebApi.JobsScheduler;
using SortingWebApi.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        private readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(10); //TODO: move to configuration and consider to use Absolute relative expiration for cache entries.


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
                    command.JobPayload, new JobSchedulingOptions() {SlidingExpiration = _slidingExpiration });

                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(job));

                // Place jobDescriptor into Cache
                await _cache.SetAsync(job.Id, bytes,
                    new DistributedCacheEntryOptions() {SlidingExpiration = job.JobSchedulingOptions.SlidingExpiration},
                    cancellationToken);

                // Schedule jobEvent for notifying workers
                await _jobsQueue.Schedule(new JobEvent { Id = job.Id, JobType = job.JobType }, cancellationToken);

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
