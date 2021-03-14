using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JobsWebApiService.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SortingWebApi.Commands;
using SortingWebApi.JobsIterator;
using SortingWebApi.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SortingWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SortingController : ControllerBase
    {
        private readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(20);
        private const string AscSortJobType = "sorting_asc";
        private readonly ILogger<SortingController> _logger;
        private readonly IScheduleJobCommandHandler _commandHandler;
        private readonly IQuery<JobsStatisticsRequest, JobDescriptor> _getJobsQuery;

        public SortingController(ILogger<SortingController> logger, IScheduleJobCommandHandler commandHandler,
            IQuery<JobsStatisticsRequest, JobDescriptor> getJobsQuery)
        {
            _logger = logger;
            _commandHandler = commandHandler;
            _getJobsQuery = getJobsQuery;
        }

        [HttpPost("SortAsync")]
        public async Task<ScheduleJobResponse> SortAsync([FromBody] ScheduleJobRequest job)
        {
            _logger.LogDebug("Job request received.");

            var command = new ScheduleJobCommand {
                JobType = AscSortJobType, 
                JobPayload = JsonSerializer.Serialize(job.Payload)
            };

            //TODO: implement proper cancellation handling
            var jobDescriptor = await _commandHandler.HandleCommand(command, CancellationToken.None);

            return new ScheduleJobResponse() { Id = jobDescriptor.Id };
        }

        [HttpGet("{id}")]
        public ScheduleJobResponse Get(string id)
        {
            return new ScheduleJobResponse() {Id = id};
        }

        [HttpGet]
        [Route("/DummyData")]
        public Task<ScheduleJobResponse> GenerateDummyData()
        {
            return SortAsync(new ScheduleJobRequest() {Payload = new int[] {1, 8, 10, 100, 5}});
        }

        [HttpGet]
        [Route("/")]
        public async IAsyncEnumerable<JobDescriptor> Index()
        {
            var cts = new CancellationTokenSource(_requestTimeout);
            IAsyncEnumerable<JobDescriptor>? asyncEnumResults;
            try
            {
                asyncEnumResults = _getJobsQuery.ExecuteRequest(new JobsStatisticsRequest(AscSortJobType), cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed during iterating through jobs.");
                throw;
            }

            await foreach (var jobDescriptor in asyncEnumResults.WithCancellation(cts.Token))
            {
                yield return jobDescriptor;
            }
        }
    }

    public class JobsStatisticsQuery: IQuery<JobsStatisticsRequest, JobDescriptor>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<JobsStatisticsQuery> _logger;
        private readonly IPendingJobIdsIterator _pendingJobIds;

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

    public class JobsStatisticsRequest : IRequest
    {
        public JobsStatisticsRequest(string jobType)
        {
            JobType = jobType;
        }

        public string JobType { get; }
    }

    public interface IQuery<in TRequest, out TResponse>
    {
        IAsyncEnumerable<TResponse> ExecuteRequest(TRequest request, CancellationToken cancellationToken);
    }

    //public class PagedResults<T>
    //{
    //    public PagedResults(IAsyncEnumerable<QueryPageResult<T>> pages)
    //    {
    //        Pages = pages;
    //    }

    //    public IAsyncEnumerable<QueryPageResult<T>> Pages { get;  }
    //}

    //public class QueryPageResult<T>
    //{
    //    public QueryPageResult(IEnumerable<T> items)
    //    {
    //        Items = items;
    //    }
    //    public IEnumerable<T> Items { get; }
    //}

    public interface IRequest
    {
    }

    //public class PageInfo
    //{
    //}

    public class ScheduleJobRequest
    {
        public int[] Payload { get; set; }
    }
}
