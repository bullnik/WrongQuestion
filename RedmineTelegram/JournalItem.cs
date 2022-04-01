using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class JournalItem
    {
        public JournalItem(int issueId, int userId, string comment, bool isComment)
        {
            IssueId = issueId;
            UserId = userId;
            Comment = comment;
            IsComment = isComment;
        }

        public int IssueId { get; set; }
        public string Comment { get; set; }
        public bool IsComment { get; }
        public int UserId { get; set; }

        
    }
}
