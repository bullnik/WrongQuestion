using System;
using System.Collections.Generic;
using System.Text;

namespace WrongQuestion
{
    public class RedmineTask
    {
        public int Id { get; private set; }
        public Tracker Tracker { get; private set; }
        public string Topic { get; private set; }
        public DateTime DateTime { get; private set; }
        public Status Status { get; private set; }
        public string Description { get; private set; }
        public List<Comment> Comments { get; private set; }

        public RedmineTask(int id, Tracker tracker, string topic, 
            DateTime dateTime, Status status, string description,
            List<Comment> comments)
        {
            Id = id;
            Tracker = tracker;
            Topic = topic;
            DateTime = dateTime;
            Status = status;
            Description = description;
            Comments = comments;
        }
    }
}
