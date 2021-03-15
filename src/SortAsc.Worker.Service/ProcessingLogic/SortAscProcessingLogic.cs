using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service.ProcessingLogic
{
    public class SortAscProcessingLogic: IJobProcessingLogic
    {
        /// <inheritdoc />
        public async Task ExecuteAsync(JobDescriptor job, CancellationToken cancellationToken)
        {
            var arrayToSort = JsonConvert.DeserializeObject<int[]>(job.Payload);
           
            // simulating long running job
            await Task.Delay(20000, cancellationToken).ConfigureAwait(false);

            Array.Sort(arrayToSort);

            job.Result = JsonConvert.SerializeObject(arrayToSort);
            job.Status = JobStatus.Finished;
        }
    }
}