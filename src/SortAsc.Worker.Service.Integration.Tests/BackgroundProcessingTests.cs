using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using SortAsc.Worker.Service.JobsQueue;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service.Integration.Tests
{
    [TestFixture]
    public class BackgroundProcessingTests
    {
        private WebApplicationFactory<Startup> _factory;
        private IOptions<KafkaJobsQueueOptions> _kafkaJobsQueueOptions;
        private TestsKafkaJobsQueue _testJobScheduler;
        private IJobsListener _jobsListener;

        [OneTimeSetUp]
        public void OneTimeSetUpAsync()
        {
            _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                // Don't run IHostedServices when running as a test
                builder.ConfigureTestServices((services) => { services.RemoveAll(typeof(IHostedService)); });

            });

            _kafkaJobsQueueOptions = _factory.Services.GetService<IOptions<KafkaJobsQueueOptions>>();

            _testJobScheduler = new TestsKafkaJobsQueue(_kafkaJobsQueueOptions?.Value);

            _jobsListener = _factory.Services.GetRequiredService<IJobsListener>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _testJobScheduler?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task BackgroundProcessing_JobInQueue_JobEventIsReadAndSentToProcessing()
        {
            // Arrange
            var job = new JobEvent(){Id = Guid.NewGuid().ToString(), JobType = _kafkaJobsQueueOptions.Value.TopicName!};

            var logic = Substitute.For<IJobProcessingLogic>();
            var logger = Substitute.For<ILogger<BackgroundJobProcessing>>();

            TimeSpan timeout = TimeSpan.FromSeconds(2);
            var cst = new CancellationTokenSource(timeout);

            //Act
            var sut = new BackgroundJobProcessing(_jobsListener, logic, logger);
            var task = sut.StartAsync(cst.Token);
            // populate job for processing
            await _testJobScheduler.Schedule(job, CancellationToken.None).ConfigureAwait(false);

            await Task.Delay(timeout);

            // Assert
            await logic.Received(1).ExecuteAsync(Arg.Any<JobEvent>(), Arg.Any<CancellationToken>());
        }
    }
}
