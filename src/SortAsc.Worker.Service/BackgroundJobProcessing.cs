using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SortAsc.Worker.Service.JobsQueue;

namespace SortAsc.Worker.Service
{
    public class BackgroundJobProcessing: BackgroundService 
    {
        private readonly IJobsListener _queueListener;
        private readonly IJobProcessingLogic _jobProcessingLogic;
        private readonly ILogger<BackgroundJobProcessing> _logger;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(3); // TODO: move to configuration

        public BackgroundJobProcessing(
            IJobsListener queueListener,
            IJobProcessingLogic jobProcessingLogic, 
            ILogger<BackgroundJobProcessing> logger)
        {
            _queueListener = queueListener;
            _jobProcessingLogic = jobProcessingLogic;
            _logger = logger;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // this yield is necessary to avoid blocking of host execution. see https://github.com/dotnet/runtime/issues/36063#issuecomment-518627708
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var jobEvent = await _queueListener.GetNextJob(stoppingToken);

                    if (jobEvent == null)
                    {
                        continue;
                    }

                    // process job
                    await _jobProcessingLogic.ExecuteAsync(jobEvent, stoppingToken).ConfigureAwait(false);

                    _queueListener.CommitProcessedJobs();

                    _logger.LogDebug("Successfully processed job: " + jobEvent.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "Failed during job processing. There will be another attempt to process requested work.",
                        ex);
                    await Task.Delay(_retryDelay, stoppingToken).ConfigureAwait(false);
                }
            }

            _logger.LogInformation("BackgroundJobProcessing gracefully finished.");
        }
    }

    public class ServiceOptions
    {
    }
}
