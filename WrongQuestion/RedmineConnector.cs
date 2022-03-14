using System;
using System.Collections.Generic;
using System.Text;

namespace WrongQuestion
{
    public class RedmineConnector
    {
        public RedmineConnector()
        {

        }

        public bool ChangeStatus(long taskId, string status)
        {
            return false;
        }

        public bool AttachComment(long taskId, string comment)
        {
            return false;
        }

        public bool CheckLogin(string telegramUsername, string redmineUsername)
        {
            return true;
        }
    }
}
