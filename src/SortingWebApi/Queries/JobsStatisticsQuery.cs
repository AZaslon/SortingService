using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SortingWebApi.JobsIterator;
using SortingWebApi.Model;

namespace SortingWebApi.Queries
{
    /// <summary>
    /// Implements Jobs statistic query.
    /// </summary>
    public class JobsStatisticsQuery: IQuery<JobsStatisticsRequest, IAsyncEnumerable<JobDescriptor>>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<JobsStatisticsQuery> _logger;
        private readonly IPendingJobIdsIterator _pendingJobIds;

        /// <summary>
        /// JobsStatisticsQuery type constructor
        /// </summary>
        /// <param name="pendingJobIds">Job Ids iterator.</param>
        /// <param name="cache">Instance of IDistributed cache.</param>
        /// <param name="logger">Logger.</param>
        public JobsStatisticsQuery(IPendingJobIdsIterator pendingJobIds, IDistributedCache cache, ILogger<JobsStatisticsQuery> logger)
        {
            _pendingJobIds = pendingJobIds;
            _cache = cache;
            _logger = logger;
        }

        //TODO: Implement pagination
        /// <inheritdoc />
        public async  IAsyncEnumerable<JobDescriptor> ExecuteRequest(JobsStatisticsRequest request,  [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var id in _pendingJobIds.GetJobIds(request.JobType, cancellationToken))
            {
                if (id != null)
                {
                    JobDescriptor job = new JobDescriptor(id, request.JobType, DateTime.MinValue, string.Empty, null, JobStatus.Failed, "Orphan job detected. Scheduled Job does not exists");
                    
                    try
                    {
                        //TODO: retry is necessary
                        var bytes = await _cache.GetAsync(id, cancellationToken);
                        if (bytes != null)
                        {
                            job = JsonConvert.DeserializeObject<JobDescriptor>(Encoding.UTF8.GetString(bytes));
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to obtain Job status");
                        job.ErrorMsg = "Failed to obtain status: " + e.Message;
                    }

                    yield return job;
                }
            }
        }
    }
}