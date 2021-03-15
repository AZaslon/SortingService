using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SortingWebApi.JobsIterator;
using SortingWebApi.Model;

namespace SortingWebApi.Queries
{
    /// <summary>
    /// Implements Job statistic query.
    /// </summary>
    public class GetJobStatisticByIdQuery : IQuery<GetJobStatisticByIdRequest, Task<JobDescriptor>>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<JobsStatisticsQuery> _logger;

        /// <summary>
        /// GetJobStatisticByIdQuery type constructor
        /// </summary>
        /// <param name="cache">Instance of IDistributed cache.</param>
        /// <param name="logger">Logger.</param>
        public GetJobStatisticByIdQuery( IDistributedCache cache, ILogger<JobsStatisticsQuery> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        //TODO: Implement pagination
        /// <inheritdoc />
        public async  Task<JobDescriptor> ExecuteRequest(GetJobStatisticByIdRequest request, CancellationToken cancellationToken)
        {
                    JobDescriptor job = new JobDescriptor(request.Id, request.JobType, DateTime.MinValue, string.Empty, null, JobStatus.Failed, "Job does not exist");
                    
                    try
                    {
                        //TODO: retry is necessary
                        var bytes = await _cache.GetAsync(request.Id, cancellationToken);
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

                    return job;
        }
    }
}