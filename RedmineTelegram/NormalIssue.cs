
namespace RedmineTelegram
{
    public class NormalIssue
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string CreatedOn { get; set; }
        public int EstimatedHours { get; set; }
        public string ClosedOn { get; set; }

        public NormalIssue(int id, string subject, string description, string status, string priority, string createdOn, int estimatedHours, string closedOn)
        {
            Id = id;
            Subject = subject;
            Description = description;
            Status = status;
            Priority = priority;
            CreatedOn = createdOn;
            EstimatedHours = estimatedHours;
            ClosedOn = closedOn;
        }
    }
}
