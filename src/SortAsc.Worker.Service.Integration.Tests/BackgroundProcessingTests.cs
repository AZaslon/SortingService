using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using SortAsc.Worker.Service.JobsQueue;
using SortAsc.Worker.Service.Model;
using SortingWebApi.Common;

namespace SortAsc.Worker.Service.Integration.Tests
{
    [TestFixture]
    public class BackgroundProcessingTests
    {
        private WebApplicationFactory<Startup> _factory;
        private IOptions<KafkaJobsQueueOptions> _kafkaJobsQueueOptions;
        private TestsKafkaJobsQueue _testJobScheduler;
        private IJobsListener _jobsListener;
        private IDistributedCache _cache;
        private ILogger<BackgroundJobProcessing> _logger;
        private IRetryPolicyFactory _retryPolicy;

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

            _cache = _factory.Services.GetRequiredService<IDistributedCache>();

            _logger = _factory.Services.GetRequiredService<ILogger<BackgroundJobProcessing>>();

            _retryPolicy = _factory.Services.GetRequiredService<IRetryPolicyFactory>();
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
            var job = new JobEvent(Guid.NewGuid().ToString(), _kafkaJobsQueueOptions.Value.TopicName!);

            var logic = Substitute.For<IJobProcessingLogic>();
            var host = Substitute.For<IHostApplicationLifetime>();

            TimeSpan timeout = TimeSpan.FromSeconds(5);
            var cst = new CancellationTokenSource(timeout);

            //Act
            var sut = new BackgroundJobProcessing(_jobsListener, logic, _cache,  host, _retryPolicy, _logger);
            var startTask = sut.StartAsync(CancellationToken.None);
            // populate job for processing
            await _testJobScheduler.Schedule(job, CancellationToken.None).ConfigureAwait(false);

            await Task.Delay(timeout);

            await sut.StopAsync(cst.Token);  // bring exceptions to main thread

            // Assert
            await logic.Received(1).ExecuteAsync(Arg.Any<JobDescriptor>(), Arg.Any<CancellationToken>());
            await logic.Received(1).ExecuteAsync(Arg.Is<JobDescriptor>(e => e.Id == job.Id && e.JobType == job.JobType), Arg.Any<CancellationToken>());
            //TODO: check other fields in assert
        }
    }

  
}
