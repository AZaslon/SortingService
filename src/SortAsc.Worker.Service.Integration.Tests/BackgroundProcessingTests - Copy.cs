using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service.Integration.Tests
{
    [TestFixture]
    public class JobProcessingLogicTests
    {
        private WebApplicationFactory<Startup> _factory;
        private IDistributedCache _cache;
        private IJobProcessingLogic _logic;

        [OneTimeSetUp]
        public void OneTimeSetUpAsync()
        {
            _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                // Don't run IHostedServices when running as a test
                builder.ConfigureTestServices((services) => { services.RemoveAll(typeof(IHostedService)); });

            });

            _cache = _factory.Services.GetService<IDistributedCache>();

            _logic = _factory.Services.GetService<IJobProcessingLogic>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _factory?.Dispose();
        }

        [Test]
        public async Task BackgroundProcessing_JobInQueue_JobEventIsReadAndSentToProcessing()
        {
            // Arrange
            var jobType = "sorting_asc";

            var jobEvent = new JobEvent{Id = Guid.NewGuid().ToString(), JobType = jobType};

            var logic = Substitute.For<IJobProcessingLogic>();
            var logger = Substitute.For<ILogger<BackgroundJobProcessing>>();

            //Act
            await _logic.ExecuteAsync(jobEvent, CancellationToken.None);

            // Assert
            
        }
    }
}
