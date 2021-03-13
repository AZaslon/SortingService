using System.Threading;
using System.Threading.Tasks;
using JobsWebApiService.Commands;

namespace SortingWebApi.Commands
{
    public interface IScheduleJobCommandHandler
    {
        public Task<JobDescriptor> HandleCommand(ScheduleJobCommand command, CancellationToken cancellationToken);
    }
}