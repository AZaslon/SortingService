using System.Threading;
using System.Threading.Tasks;
using SortingWebApi.Model;

namespace SortingWebApi.JobsScheduler
{
    public interface IJobsQueue
    {
        /// <summary>
        /// This method adds job to processing queue.
        /// </summary>
        /// <param name="jobEvent">Descriptor of scheduled job.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task Schedule(JobEvent jobEvent, CancellationToken cancellationToken);
    }
}