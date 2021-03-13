using System;
using SortingWebApi.Common;

namespace SortingWebApi.Model
{
    public class JobDescriptor
    {
        public JobDescriptor(string id, string jobType, DateTime createdDateTime, string payload,
            JobSchedulingOptions jobSchedulingOptions)
        {
            Id = id;
            JobType = jobType;
            CreatedDateTime = createdDateTime;
            Payload = payload;
            JobSchedulingOptions = jobSchedulingOptions ?? new JobSchedulingOptions();
        }

        public string Id { get; set; }
        public string JobType { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Payload { get; set;  }
        public JobSchedulingOptions JobSchedulingOptions { get; set; }
    }
}