using System;
using System.Threading;
using System.Threading.Tasks;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service.JobsQueue
{
    public interface IJobsListener: IDisposable
    {
        Task<JobEvent?> GetNextJob(CancellationToken cancellationToken);

        void CommitProcessedJobs();
    }
}