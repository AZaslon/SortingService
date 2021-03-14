using System;
using System.Text;
using System.Text.Json;
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
        public async Task BackgroundProcessing_ValidJobDescriptor_PayloadIsProcessed()
        {
            // Arrange
            var jobType = "sorting_asc";

            var job = new JobDescriptor(
                Guid.NewGuid().ToString(), 
                jobType, DateTime.UtcNow, 
                "[1,3,2,6,3]",
                new JobSchedulingOptions() { SlidingExpiration = TimeSpan.FromMinutes(10)});

            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));

            // Place job into Cache
            await _cache.SetAsync(job.Id, bytes,
                new DistributedCacheEntryOptions() { SlidingExpiration = job.JobSchedulingOptions.SlidingExpiration },
                CancellationToken.None);

            //Act
            await _logic.ExecuteAsync(job, CancellationToken.None);

            // Assert
            
        }
    }
}
