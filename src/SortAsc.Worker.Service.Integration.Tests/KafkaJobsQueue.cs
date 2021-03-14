using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SortAsc.Worker.Service.JobsQueue;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service.Integration.Tests
{
    public class TestsKafkaJobsQueue :  IDisposable
    {
        private readonly KafkaJobsQueueOptions _options;
        private readonly IProducer<string, byte[]> _producer;

        public TestsKafkaJobsQueue(KafkaJobsQueueOptions options)
        {
            _options = options;

            _producer = Build(_options);
        }


        /// <inheritdoc />
        public async Task Schedule(JobEvent jobEvent, CancellationToken cancellationToken)
        {
            var cloudEvent = ToCloudEvent(jobEvent);
            
            var message = new KafkaCloudEventMessage(cloudEvent, ContentMode.Structured, new JsonEventFormatter());

            await Produce(_options.TopicName, message, cancellationToken).ConfigureAwait(false);
        }

        public CloudEvent ToCloudEvent(JobEvent jobEvent)
        {
            if (jobEvent == null)
            {
                throw new ArgumentNullException(nameof(jobEvent));
            }

            var cloudEvent = new CloudEvent(
                CloudEventsSpecVersion.V1_0,
                jobEvent.JobType,
                new Uri("http://dummyUri"),
                jobEvent.Id,
                DateTime.UtcNow)
            {
                Data = jobEvent
            };

            return cloudEvent;
        }


        public IProducer<string, byte[]> Build(KafkaJobsQueueOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var config = CreateProducerConfig(options);

            var producerBuilder = new ProducerBuilder<string, byte[]>(config);

            return producerBuilder.Build();
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }

        private static ProducerConfig CreateProducerConfig(KafkaJobsQueueOptions options)
        {
            return new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                SecurityProtocol = options.SecurityProtocol,
                SaslMechanism = options.SaslMechanism,
                SaslUsername = options.SaslUsername,
                SaslPassword = options.SaslPassword,
                EnableDeliveryReports = true,
                Acks = Acks.All,
                LogConnectionClose = false,

                // This article (https://docs.microsoft.com/en-us/azure/event-hubs/apache-kafka-configurations) recommends setting for this values:
                SocketKeepaliveEnable = true,
                MetadataMaxAgeMs = 180000,
                RequestTimeoutMs = 30000
            };
        }

        private async Task Produce(string topic, KafkaCloudEventMessage message, CancellationToken cancellationToken)
        {
            if (_producer == null)
            {
                return;
            }

            var deliveryResult = await _producer.ProduceAsync(topic, message, cancellationToken).ConfigureAwait(false);

            if (deliveryResult.Status != PersistenceStatus.Persisted)
            {
                throw new ApplicationException($"job scheduling error: Queue returned unexpected persistence status {deliveryResult.Status}");
            }
        }

    }
}
