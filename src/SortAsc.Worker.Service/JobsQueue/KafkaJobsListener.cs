using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SortAsc.Worker.Service.Model;

namespace SortAsc.Worker.Service.JobsQueue
{
    public class KafkaJobsListener : IJobsListener
    {
        private IConsumer<string, byte[]>? _consumer;
        private readonly IMessageConverter _converter;
        private readonly ILogger<KafkaJobsListener> _logger;

        public KafkaJobsListener(IOptions<KafkaJobsQueueOptions> options, IMessageConverter converter, ILogger<KafkaJobsListener> logger)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            _consumer = new ConsumerBuilder<string, byte[]>(CreateConsumerConfig(options.Value)).Build();
            _consumer.Subscribe(options.Value.TopicName);
            _converter = converter;
            _logger = logger;
        }

        public Task<JobEvent?> GetNextJob(CancellationToken cancellationToken)
        {
            _consumer = _consumer ?? throw new ApplicationException("The instance of consumer is already disposed");
            
            var consumeResult = _consumer.Consume(cancellationToken);
            
            _logger.LogDebug($"Converting message.");

            if (_converter.TryConvert(consumeResult.Message, out JobEvent? jobDescriptor))
            {
                _logger.LogDebug($"Message converted for job with id {jobDescriptor?.Id} and type {jobDescriptor?.JobType}");

                return Task.FromResult(jobDescriptor);
            }

            _logger.LogDebug($"Failed to convert message with key '{consumeResult.Message.Key ?? string.Empty}'.");

            return Task.FromResult<JobEvent?>(null);
        }

        public void CommitProcessedJobs()
        {
            _consumer?.Commit();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _consumer?.Unsubscribe();
            _consumer?.Close();
            _consumer?.Dispose();
            _consumer = null;
        }

        private static ConsumerConfig CreateConsumerConfig(KafkaJobsQueueOptions options)
        {
            return new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                SaslMechanism = options.SaslMechanism,
                SecurityProtocol = options.SecurityProtocol,
                SaslUsername = options.SaslUsername,
                SaslPassword = options.SaslPassword,
                GroupId = "worker",
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
    }
}
