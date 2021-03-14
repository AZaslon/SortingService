using System;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SortingWebApi.Model;

namespace SortingWebApi.JobsQueueListener
{
    public interface IMessageConverter
    {
        bool TryConvert(Message<string, byte[]> message, out JobEvent? jobEvent);
    }

    public class MessageConverter : IMessageConverter
    {
        private readonly ILogger<MessageConverter> _logger;

        public MessageConverter(ILogger<MessageConverter> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public bool TryConvert(Message<string, byte[]> message, out JobEvent? jobEvent)
        {
            try
            {
                if (!message.IsCloudEvent())
                {
                    _logger.LogDebug($"Received message is not a cloud event. Message key: '{message.Key}'.");

                    jobEvent = null;
                    return false;
                }

                var cloudEvent = message.ToCloudEvent(new JsonEventFormatter());

                jobEvent = (cloudEvent.Data as JObject)?.ToObject<JobEvent>();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Received message cannot be parsed to JobDescription. Message key: '{message.Key}'.", ex);
            }

            jobEvent = null;
            return false;
        }
    }
}