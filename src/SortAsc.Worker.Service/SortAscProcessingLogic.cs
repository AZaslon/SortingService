using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service
{
    public class SortAscProcessingLogic: IJobProcessingLogic
    {
        /// <inheritdoc />
        public async Task ExecuteAsync(JobDescriptor jobEvent, CancellationToken cancellationToken)
        {

            await Task.Delay(2000).ConfigureAwait(false);



        }
    }
}