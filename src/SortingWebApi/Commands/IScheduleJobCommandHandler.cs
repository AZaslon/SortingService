using System.Threading;
using System.Threading.Tasks;
using SortingWebApi.Common;
using SortingWebApi.Model;

namespace SortingWebApi.Commands
{
    public interface IScheduleJobCommandHandler
    {
        public Task<JobDescriptor> HandleCommand(ScheduleJobCommand command, CancellationToken cancellationToken);
    }
}