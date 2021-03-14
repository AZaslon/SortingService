using System;
using System.Collections.Generic;
using System.Threading;

namespace SortingWebApi.JobsIterator
{
    public interface IPendingJobIdsIterator: IDisposable
    {
        /// <summary>
        /// Returns ids of pending jobs.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IEnumerable<string?> GetJobIds(string jobType, CancellationToken cancellationToken);
    }
}