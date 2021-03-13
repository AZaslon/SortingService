using System.Threading;
using System.Threading.Tasks;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service
{
    public interface IJobProcessingLogic
    {
        Task ExecuteAsync(JobEvent job, CancellationToken cancellationToken);
    }
}