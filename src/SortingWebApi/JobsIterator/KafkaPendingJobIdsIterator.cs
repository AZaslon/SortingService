using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SortingWebApi.JobsQueueListener;
using SortingWebApi.JobsScheduler;
using SortingWebApi.Model;

namespace SortingWebApi.JobsIterator
{
    public class KafkaPendingJobIdsIterator : IPendingJobIdsIterator
    {
        private IConsumer<string, byte[]>? _consumer;
        private readonly IMessageConverter _converter;
        private readonly ILogger<KafkaPendingJobIdsIterator> _logger;
        //TODO: figure out why consumer requires this warmup time?
        // potentially replace it with retry.
        readonly TimeSpan waitForConsumer = TimeSpan.FromSeconds(2);
        public KafkaPendingJobIdsIterator(IOptions<KafkaJobsQueueOptions> options, IMessageConverter converter, ILogger<KafkaPendingJobIdsIterator> logger)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            _consumer = new ConsumerBuilder<string, byte[]>(CreateConsumerConfig(options.Value)).Build();
            
            _converter = converter;
            _logger = logger;
        }
        /// <inheritdoc />
        public  IEnumerable<string?> GetJobIds(string jobType, CancellationToken cancellationToken)
        {
            _consumer = _consumer ?? throw new ApplicationException("The instance of consumer is already disposed");
            _consumer.Subscribe(jobType);
           
                ConsumeResult<string, byte[]>? consumeResult = null;

                do
                {
                    try
                    {
                        consumeResult = _consumer.Consume(waitForConsumer);
                    }
                    catch (ConsumeException e) when (e.Message == "Broker: Unknown topic or partition")
                    {
                        // there is no data yet
                        yield break;
                    }

                    if (consumeResult == null)
                    {
                        continue;
                    }

                    _logger.LogDebug($"Converting message.");

                    if (_converter.TryConvert(consumeResult.Message, out JobEvent? jobEvent))
                    {
                        _logger.LogDebug(
                            $"Message converted for job with id {jobEvent?.Id} and type {jobEvent?.JobType}");

                        yield return jobEvent?.Id;
                    }

                    _logger.LogDebug(
                        $"Failed to convert message with key '{consumeResult.Message.Key ?? string.Empty}'.");

                } while (!cancellationToken.IsCancellationRequested && consumeResult != null);
          
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

                GroupId = Guid.NewGuid().ToString(), // new group created each time to iterate through all messages and avoid group coordination

                AutoOffsetReset = AutoOffsetReset.Earliest,
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
