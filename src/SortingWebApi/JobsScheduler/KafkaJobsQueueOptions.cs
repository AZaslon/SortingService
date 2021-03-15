
using System.ComponentModel.DataAnnotations;
using Confluent.Kafka;

namespace SortingWebApi.JobsScheduler
{
    public class KafkaJobsQueueOptions
    {
        public const string Key = "KafkaJobsQueueOptions";

        /// <summary>
        /// Gets or sets the bootstrap servers.
        /// </summary>
        [Required(AllowEmptyStrings = false)] 
        public string? BootstrapServers { get; set; }

        /// <summary>
        /// Gets or sets SASL user name.
        /// </summary>
        public string? SaslUsername { get; set; }

        /// <summary>
        /// Gets or sets SASL password.
        /// </summary>
        public string? SaslPassword { get; set; }

        /// <summary>
        /// Gets or sets security protocol.
        /// </summary>
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;

        /// <summary>
        /// Gets or sets Sasl mechanism.
        /// </summary>
        public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.Plain;
    }
}