
namespace SortingWebApi.Queries
{
    /// <summary>
    /// This type implements job status request details 
    /// </summary>
    public class GetJobStatisticByIdRequest
    {
        /// <summary>
        /// GetJobStatisticByIdRequest constructor.
        /// </summary>
        /// <param name="jobType">Job type.</param>
        /// <param name="id">Unique identifier.</param>
        public GetJobStatisticByIdRequest(string jobType, string id)
        {
            JobType = jobType;
            Id = id;
        }

        /// <summary>
        /// Requested Job Type.
        /// </summary>
        public string JobType { get; }

        /// <summary>
        /// Requested Job Id.
        /// </summary>
        public string Id { get; }
    }
}