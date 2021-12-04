using System;

namespace WrongQuestion
{
    public class Comment
    {
        public RedmineUser Author { get; private set; }
        public DateTime DateTime { get; private set; }
        public string Content { get; private set; }

        public Comment(RedmineUser author, DateTime dateTime, string content)
        {
            Author = author;
            DateTime = dateTime;
            Content = content;
        }
    }
}