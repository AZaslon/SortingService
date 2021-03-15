using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SortAsc.Worker.Service.Model
{

    public enum JobStatus
    {
        Created = 10,

        InProgress = 20,

        Finished = 30,

        Failed = 50
    }

    public class JobDescriptor
    {
        public JobDescriptor(string id, string jobType, DateTime createdDateTime, string payload,
            JobSchedulingOptions? jobSchedulingOptions = null, JobStatus status = JobStatus.Created, string? errorMsg = null)
        {
            Id = id;
            JobType = jobType;
            CreatedDateTime = createdDateTime;
            Payload = payload;
            Status = status;
            ErrorMsg = errorMsg;
            JobSchedulingOptions = jobSchedulingOptions ?? new JobSchedulingOptions() {SlidingExpiration = TimeSpan.FromMinutes(10)};
        }

        public string Id { get; set; }
        public string JobType { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Payload { get; set;  }
        public JobSchedulingOptions JobSchedulingOptions { get; set; }
        public DateTime LastUpdated { get; set; }
        public JobStatus Status { get; set; } 
        public string? ErrorMsg { get; set; }
        public string? Result { get; set; }
    }
}