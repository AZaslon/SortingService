using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service.Integration.Tests
{
    [TestFixture]
    public class JobProcessingLogicTests
    {
        private WebApplicationFactory<Startup> _factory;
        private IJobProcessingLogic _logic;

        [OneTimeSetUp]
        public void OneTimeSetUpAsync()
        {
            _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                // Don't run IHostedServices when running as a test
                builder.ConfigureTestServices((services) => { services.RemoveAll(typeof(IHostedService)); });

            });

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
            //TODO: move this test to UnitTest as it does not concern integration aspects
            // Arrange
            var jobType = "sorting_asc";

            var job = new JobDescriptor(
                Guid.NewGuid().ToString(), 
                jobType, DateTime.UtcNow, 
                "[1,3,2,6,3]",
                new JobSchedulingOptions() { SlidingExpiration = TimeSpan.FromMinutes(10)});

            //Act
            await _logic.ExecuteAsync(job, CancellationToken.None);

            // Assert
            Assert.AreEqual("[1,2,3,3,6]", job.Result);
        }
    }
}
