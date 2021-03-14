
namespace SortAsc.Worker.Service.Model
{
    public class JobEvent
    {
        public JobEvent(string id, string jobType)
        {
            Id = id;
            JobType = jobType;
        }

        public string Id { get; set; }
        public string JobType { get; set; }
    }
}