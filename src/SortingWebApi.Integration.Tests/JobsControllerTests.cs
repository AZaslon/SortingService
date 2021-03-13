﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using JobsWebApiService;
using JobsWebApiService.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SortingWebApi.JobsScheduler;
using SortingWebApi.Models;

namespace SortingWebApi.Integration.Tests
{
    [TestFixture]
    public class JobsControllerTests
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;
        private KafkaJobsQueueOptions _kafkaJobsQueueOptions;
        private ConsumerConfig _consumerConfig;

        [OneTimeSetUp]
        public void OneTimeSetUpAsync()
        {
            _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder => { });
            _client = _factory.CreateClient();

            _kafkaJobsQueueOptions = _factory.Services.GetService<IOptions<KafkaJobsQueueOptions>>()?.Value ?? throw new ArgumentNullException(nameof(IOptions<KafkaJobsQueueOptions>.Value));
            
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaJobsQueueOptions.BootstrapServers,
                SaslMechanism = _kafkaJobsQueueOptions.SaslMechanism,
                SecurityProtocol = _kafkaJobsQueueOptions.SecurityProtocol,
                SaslUsername = _kafkaJobsQueueOptions.SaslUsername,
                SaslPassword = _kafkaJobsQueueOptions.SaslPassword,
                GroupId = "IntegrationTestsGroup",
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = false,
                LogConnectionClose = false,

                // This article (https://docs.microsoft.com/en-us/azure/event-hubs/apache-kafka-configurations) recommends setting these values:
                SocketKeepaliveEnable = true,
                MetadataMaxAgeMs = 180000,
                HeartbeatIntervalMs = 3000,
                SessionTimeoutMs = 30000,
                MaxPollIntervalMs = 300000
            };

        }


        [Test]
        public async Task ScheduleJob_ValidInput_ReturnsJobObject()
        {
            // Act
            var response = await _client.GetAsync("api/jobs/get/testJobId").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.AreEqual("{\"id\":\"testJobId\"}", responseString);
        }

        [Test]
        public async Task ScheduleJob_ValidInput_JobIsQueued()
        {



            // Act
            var response = await _client.GetAsync("api/jobs/get/testJobId").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();



            // Assert
            Assert.AreEqual("{\"id\":\"testJobId\"}", responseString);
        }

        [Test]
        public async Task WriteAsync_ValidConfigurations_MessageIsInKafka()
        {
            //Arrange
            var request = new 
            {
                Payload = new[] {9, 8, 7, 6, 5, 4, 3, 2, 1}
            };

            var stringContent = new StringContent(JObject.FromObject(request).ToString(), Encoding.UTF8, "application/json");
            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
            consumer.Subscribe("sorting_asc");

            //Act
            var response = await _client.PostAsync("api/sorting/sortasync", stringContent).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync();

            //Assert
            Assert.IsNotNull(responseString);


            dynamic scheduleJobResponse = JsonConvert.DeserializeObject<JObject>(responseString);

            Assert.IsNotNull(scheduleJobResponse);
            Assert.IsNotEmpty(scheduleJobResponse.Id);

            try
            {
                var cr = consumer.Consume(5000);
                Assert.IsNotNull(cr, "Job Event is expected.");
                Assert.IsNotNull(cr.Message?.Value);
            }
            finally
            {
                consumer.Unsubscribe();
                consumer.Close();
            }

            //TODO: Add asserting to validate event 
        }
    }
}
