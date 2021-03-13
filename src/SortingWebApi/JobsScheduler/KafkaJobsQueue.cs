using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using Confluent.Kafka;
using JobsWebApiService.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SortingWebApi.Commands;
using SortingWebApi.Common;

namespace SortingWebApi.JobsScheduler
{
    //TODO: Refactoring is necessary - class looks bulky 

    /// <summary>
    /// Kafka based implementation of Job queue.
    /// </summary>
    public class KafkaJobsQueue : IJobsQueue, IDisposable
    {
        private readonly KafkaJobsQueueOptions _options;
        private readonly ILogger<KafkaJobsQueue> _logger;
        private IProducer<string, byte[]>? _producer;
        private readonly IRetryPolicyFactory _retrier;

        public KafkaJobsQueue(
            IOptions<KafkaJobsQueueOptions> options,
            ILogger<KafkaJobsQueue> logger,
            IRetryPolicyFactory retrierFactory)
        {
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value
                       ?? throw new ArgumentException($"{nameof(options)} parameter does not contain a valid value",
                           nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retrier = retrierFactory ?? throw new ArgumentNullException(nameof(retrierFactory));

            _producer = Build(_options);
        }


        /// <inheritdoc />
        public async Task Schedule(JobDescriptor jobDescriptor, CancellationToken cancellationToken)
        {
            var cloudEvent = ToCloudEvent(jobDescriptor);
            
            var message = new KafkaCloudEventMessage(cloudEvent, ContentMode.Structured, new JsonEventFormatter());

            await _retrier.Create<Exception>(_logger)
                .ExecuteAsync(() => Produce(jobDescriptor.JobType, message, cancellationToken)).ConfigureAwait(false);
        }

        public CloudEvent ToCloudEvent(JobDescriptor jobDescriptor)
        {
            if (jobDescriptor == null)
            {
                throw new ArgumentNullException(nameof(jobDescriptor));
            }

            var cloudEvent = new CloudEvent(
                CloudEventsSpecVersion.V1_0,
                jobDescriptor.JobType,
                new Uri("http://dummyUri"),
                jobDescriptor.Id,
                DateTime.UtcNow)
            {
                Data = jobDescriptor
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

                // This article (https://docs.microsoft.com/en-us/azure/event-hubs/apache-kafka-configurations) recommends setting these values:
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

            _logger.LogDebug($"Job scheduling info: Message delivered to queue '{deliveryResult.TopicPartitionOffset}'");
        }

    }
}
