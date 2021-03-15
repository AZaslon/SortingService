namespace SortingWebApi.Model
{
    /// <summary>
    /// Schedule sorting job request DTO
    /// </summary>
    public class ScheduleJobRequest
    {
        /// <summary>
        /// Payload for job.
        /// </summary>
        public int[]? Payload { get; set; }
    }
}