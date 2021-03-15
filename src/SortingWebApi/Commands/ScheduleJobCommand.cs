namespace SortingWebApi.Commands
{
    /// <summary>
    /// Job scheduling command
    /// </summary>
    public class ScheduleJobCommand
    {
        public ScheduleJobCommand(string jobPayload, string jobType)
        {
            JobPayload = jobPayload;
            JobType = jobType;
        }

        public string JobPayload { get; set; }
        public string JobType { get; set; }
    }
}
