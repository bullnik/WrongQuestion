using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class JournalItem
    {
        public JournalItem(int issueId, int userId, string userName,
            bool isComment, string comment, 
            bool isIssueStatusChange, string oldIssueStatus, string currentIssueStatus)
        {
            IssueId = issueId;
            UserId = userId;
            UserName = userName;
            IsComment = isComment;
            Comment = comment;
            IsIssueStatusChange = isIssueStatusChange;
            CurrentIssueStatus = currentIssueStatus;
            OldIssueStatus = oldIssueStatus;
        }

        public bool IsIssueStatusChange { get; private set; }
        public string CurrentIssueStatus { get; private set; }
        public string UserName { get; private set; }
        public string OldIssueStatus { get; private set; }
        public int IssueId { get; private set; }
        public string Comment { get; private set; }
        public bool IsComment { get; private set; }
        public int UserId { get; private set; }

    }
}
