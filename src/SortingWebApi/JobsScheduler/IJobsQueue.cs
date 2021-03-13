// © 2021 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Threading;
using System.Threading.Tasks;
using JobsWebApiService.Commands;
using SortingWebApi.Commands;

namespace SortingWebApi.JobsScheduler
{
    public interface IJobsQueue
    {
        /// <summary>
        /// This method adds job to processing queue.
        /// </summary>
        /// <param name="jobDescriptor">Descriptor of scheduled job.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task Schedule(JobDescriptor jobDescriptor, CancellationToken cancellationToken);
    }
}