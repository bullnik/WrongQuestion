
namespace RedmineTelegram
{
    public class NormalIssue
    {
        public int Id { get; private set; }
        public string Subject { get; private set; }
        public string Description { get; private set; }
        public string Status { get; private set; }
        public string Priority { get; private set; }
        public string CreatedOn { get; private set; }
        public int EstimatedHours { get; private set; }
        public string ClosedOn { get; private set; }
        public long AssignedTo { get; private set; }
        public long CreatorId { get; private set; }
        public double LaborCostsSum { get; private set; }
        public string CreatorName { get; private set; }
        public bool IsClosed { get; }
        public string Link { get; private set; }

        public NormalIssue(int id, string subject, string description,
            string status, string priority, string createdOn, int estimatedHours,
            string closedOn, long assignedTo, long creatorId, double laborCostsSum, string creatorName, bool isClosed)
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
            AssignedTo = assignedTo;
            CreatorId = creatorId;
            LaborCostsSum = laborCostsSum;
            CreatorName = creatorName;
            IsClosed = isClosed;
            Link = $"[{Subject}](https://ca62-77-222-118-66.ngrok.io/redmine/issues/{Id})";
        }
    }
}
