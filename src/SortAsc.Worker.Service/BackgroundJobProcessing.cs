using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SortAsc.Worker.Service.JobsQueue;
using SortAsc.Worker.Service.Model;
using SortingWebApi.Common;

namespace SortAsc.Worker.Service
{
    public class BackgroundJobProcessing: BackgroundService 
    {
        private readonly IJobsListener _queueListener;
        private readonly IJobProcessingLogic _jobProcessingLogic;
        private readonly IDistributedCache _cache;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IRetryPolicyFactory _retryPolicyFactory;
        private readonly ILogger<BackgroundJobProcessing> _logger;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(3); // TODO: move to configuration
        private readonly TimeSpan _refreshStatusDelay = TimeSpan.FromSeconds(3); // TODO: move to configuration

        public BackgroundJobProcessing(
            IJobsListener queueListener,
            IJobProcessingLogic jobProcessingLogic, 
            IDistributedCache cache,
            IHostApplicationLifetime hostApplicationLifetime,
            IRetryPolicyFactory retryPolicy,
            ILogger<BackgroundJobProcessing> logger)
        {
            _queueListener = queueListener;
            _jobProcessingLogic = jobProcessingLogic;
            _cache = cache;
            _hostApplicationLifetime = hostApplicationLifetime;
            _retryPolicyFactory = retryPolicy;
            _logger = logger;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // this yield is necessary to avoid blocking of host execution. see https://github.com/dotnet/runtime/issues/36063#issuecomment-518627708
            // Also check this article about life time aspects of background services https://blog.stephencleary.com/2020/06/backgroundservice-gotcha-application-lifetime.html
            // this issue can also be fixed by wrapping execution code in Task.Run(async () =>

            await Task.Yield();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Waiting for job.....");
                    try
                    {
                        await ProcessAndTrackJobStatus(_jobProcessingLogic, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed during job processing. There will be another attempt to process requested work.");
                        await Task.Delay(_retryDelay, stoppingToken).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("BackgroundJobProcessing gracefully finished.");
            }
            catch (Exception ex) when (False(() => _logger.LogCritical(ex, "Fatal error")))
            {
                throw;
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }

        private static bool False(Action action) { action(); return false; }

        private async Task ProcessAndTrackJobStatus(IJobProcessingLogic logic, CancellationToken stoppingToken)
        {
            var jobEvent = await _queueListener.GetNextJob(stoppingToken);

            if (jobEvent == null)
            {
                _logger.LogWarning($"Job event from queue is null, event processing is skipped.");
                return;
            }

            var bytesFromCache = await _cache.GetAsync(jobEvent.Id, stoppingToken).ConfigureAwait(false);

            if (bytesFromCache == null)
            {
                var msg = "Detected JobEvent without corresponding JobDescription! Skipping this event!";

                _logger.LogWarning(msg);
                var orphanJobDescription = new JobDescriptor(jobEvent.Id, jobEvent.JobType, DateTime.UtcNow, string.Empty);
                await TryPushJobFailedNotification(orphanJobDescription, msg, stoppingToken);
                
                // Job is skipped from processing
                _queueListener.CommitProcessedJobs();
                return;
            }

            var job = JsonConvert.DeserializeObject<JobDescriptor>(Encoding.UTF8.GetString(bytesFromCache));

            job.Status = JobStatus.InProgress;

            try
            {
                await TryUpsertJobInCache(job, stoppingToken);

                // TODO: add retrier 
                var processingTask = logic.ExecuteAsync(job, stoppingToken);


                while (!processingTask.IsCompleted)
                {
                    await Task.Delay(_refreshStatusDelay, stoppingToken).ConfigureAwait(false);
                    job.LastUpdated = DateTime.UtcNow;

                    await TryUpsertJobInCache(job, stoppingToken);

                    _logger.LogDebug($"Updated job {job.Id} status: {job.Status}");
                }

                await processingTask; // bring exceptions if any;



            }
            catch (Exception ex)
            {
                await TryPushJobFailedNotification(job, "Job failed to process! Error: " + ex.Message , stoppingToken);
            }

            _queueListener.CommitProcessedJobs();

            _logger.LogDebug("Successfully processed job: " + jobEvent.Id);
        }

        private async Task<bool> TryPushJobFailedNotification(JobDescriptor job, string failReason, CancellationToken cancellationToken)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMsg = failReason;
            return await TryUpsertJobInCache(job, cancellationToken);
        }

        private async Task<bool> TryUpsertJobInCache(JobDescriptor job, CancellationToken cancellationToken)
        {
            var bytesToCache = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(job));
            try
            {
                // Update jobDescriptor into Cache
                // TODO: add retrier 
                await _cache.SetAsync(job.Id, bytesToCache,
                    new DistributedCacheEntryOptions()
                        { SlidingExpiration = job.JobSchedulingOptions.SlidingExpiration },
                    cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot create/updated JobDescription in cache.");
            }

            return false;
        }
    }
}
