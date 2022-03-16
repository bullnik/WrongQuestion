
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
            if (subject.Length > 200)
            {
                subject = subject[..200];
            }
            Subject = subject;
            if (description.Length > 400)
            {
                description = description[..400];
            }
            Description = description;
            Status = status;
            Priority = priority;
            CreatedOn = createdOn;
            EstimatedHours = estimatedHours;
            ClosedOn = closedOn;
        }
    }
}
