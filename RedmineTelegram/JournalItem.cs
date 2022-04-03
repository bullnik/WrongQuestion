using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class JournalItem
    {
        public JournalItem(int issueId, int userId, string comment, bool isComment, 
            bool isIssueStatusChange, string currentIssueStatus)
        {
            IssueId = issueId;
            UserId = userId;
            Comment = comment;
            IsComment = isComment; 
            IsIssueStatusChange = IsIssueStatusChange;
            CurrentIssueStatus = currentIssueStatus;
        }

        public bool IsIssueStatusChange { get; set; }
        public string CurrentIssueStatus { get; }

        public int IssueId { get; set; }
        public string Comment { get; set; }
        public bool IsComment { get; }
        public int UserId { get; set; }

        
    }
}
