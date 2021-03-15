
namespace SortingWebApi.Queries
{
    /// <summary>
    /// This type implements jobs status request details 
    /// </summary>
    public class JobsStatisticsRequest
    {
        public JobsStatisticsRequest(string jobType)
        {
            JobType = jobType;
        }

        /// <summary>
        /// Requested Job Type.
        /// </summary>
        public string JobType { get; }
    }
}