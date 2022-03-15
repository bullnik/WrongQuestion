using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class IssuesUpdateChecker
    {
        private readonly InternalDatabase _internalDatabase;
        private readonly RedmineDatabase _redmineDatabase;
        private readonly TelegramBot _telegramBot;

        public IssuesUpdateChecker(InternalDatabase internalDatabase, 
            RedmineDatabase redmineDatabase, TelegramBot telegramBot)
        {
            _internalDatabase = internalDatabase;
            _redmineDatabase = redmineDatabase;
            _telegramBot = telegramBot;
        }

        public  void StartChecking()
        {
             Check();
        }

        private void Check()
        {
            while (true)
            {
                List<Issue> lastEditedIssues = _redmineDatabase.LoadLastEditedIssues(100);

                foreach (Issue issue in lastEditedIssues)
                {
                    if (_internalDatabase.TryGetIssueById(issue.Id, out Issue savedIssue)) 
                    {
                        if (issue.IsClosed && !savedIssue.IsClosed)
                        {
                            _internalDatabase.InsertOrUpdateIssue(issue);
                            SendIssueToUserIfUserFound(issue);
                        }
                    }
                    else
                    {
                        _internalDatabase.InsertOrUpdateIssue(issue);
                        SendIssueToUserIfUserFound(issue);
                    }
                }

                Thread.Sleep(2500);
            }
        }

        private void SendIssueToUserIfUserFound(Issue issue)
        {
            if (_redmineDatabase.TryGetTelegramUsernameByRedmineId(issue.AssignedTo, out string username)
                && _internalDatabase.TryGetUserTelegramIdByUsername(username, out long userId))
            {
                NormalIssue normalIssue = _redmineDatabase.GetNormalIssue(issue.Id);
                if (issue.IsClosed)
                {
                    _telegramBot.SendClosedIssueToUser(normalIssue, userId);
                }
                else
                {
                    _telegramBot.SendNewIssueToUser(normalIssue, userId);
                }
            }
        }
    }
}
