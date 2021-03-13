using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service
{
    public class SortAscProcessingLogic: IJobProcessingLogic
    {
        private readonly IDistributedCache _cache;

        public SortAscProcessingLogic(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <inheritdoc />
        public Task ExecuteAsync(JobEvent job, CancellationToken cancellationToken)
        {
            // do nothing
            return Task.CompletedTask;
        }
    }
}