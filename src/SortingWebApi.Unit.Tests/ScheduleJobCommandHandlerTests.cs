using System.Threading;
using System.Threading.Tasks;
using JobsWebApiService.Commands;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SortingWebApi.Commands;
using SortingWebApi.JobsScheduler;
using Arg = NSubstitute.Arg;

namespace SortingWebApi.Unit.Tests
{
    [TestFixture]

    public class ScheduleJobCommandHandlerTests
    {
        //TODO: tests for constructor


        [Test]
        public async Task Handle_ValidCommand_JobIsCreated()
        {
            //Arrange
            var cache = Substitute.For<IDistributedCache>();
            var queue = Substitute.For<IJobsQueue>();

            var command = new ScheduleJobCommand() {JobPayload = "Test payload", JobType = "Test job type"};
            var logger = Substitute.For<ILogger<ScheduleJobCommandHandler>>();

            var sut = new ScheduleJobCommandHandler(cache, queue, logger);

            //Act
            var job = await sut.HandleCommand(command, CancellationToken.None);

            //Assert
            Assert.IsNotNull(job.JobType);
        }


        [Test]
        public async Task Handle_ValidCommand_JobIsCached()
        {
            //Arrange
            var cache = Substitute.For<IDistributedCache>();
            var queue = Substitute.For<IJobsQueue>();
            var logger = Substitute.For<ILogger<ScheduleJobCommandHandler>>();

            var command = new ScheduleJobCommand() {JobPayload = "Test payload", JobType = "Test job type"};

            var sut = new ScheduleJobCommandHandler(cache, queue, logger);

            //Act
            var job = await sut.HandleCommand(command, CancellationToken.None);

            //Assert
            Assert.IsNotNull(job.JobType);
            await cache.Received(1)
                .SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
        }


        [Test]
        public async Task Handle_ValidCommand_EventIsCreated()
        {
            //Arrange
            var cache = Substitute.For<IDistributedCache>();
            var queue = Substitute.For<IJobsQueue>();
            var logger = Substitute.For<ILogger<ScheduleJobCommandHandler>>();

            var command = new ScheduleJobCommand() {JobPayload = "Test payload", JobType = "Test job type"};

            var sut = new ScheduleJobCommandHandler(cache, queue, logger);

            //Act
            var job = await sut.HandleCommand(command, CancellationToken.None);

            //Assert
            await queue.Received(1)
                .Schedule(Arg.Any<JobDescriptor>(), Arg.Any<CancellationToken>());
        }

    }
}
