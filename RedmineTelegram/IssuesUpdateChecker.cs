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

        public async void StartChecking()
        {
            await Task.Run(() => Check());
        }

        private void Check()
        {
            DateTime lastIssuesUpdateCheckTime = DateTime.Now;

            while (true)
            {
                List<JournalItem> lastEditedJournals = _redmineDatabase.LoadLastJournalsLine(lastIssuesUpdateCheckTime);
                List<NormalIssue> lastCreatedIssues = _redmineDatabase.LoadLastCreatedIssues(lastIssuesUpdateCheckTime);

                foreach (NormalIssue issue in lastCreatedIssues)
                {
                    List<int> watchersRedmineIds = _redmineDatabase.GetWatchersIdList(issue.Id);

                    SendIssueToUser(issue.AssignedTo, issue);
                    foreach (int watcherRedmineId in watchersRedmineIds)
                    {
                        if (watcherRedmineId == issue.AssignedTo 
                            || watcherRedmineId == issue.CreatorId)
                        {
                            continue;
                        }
                        SendIssueToUser(watcherRedmineId, issue);
                    }
                }

                foreach (JournalItem journalItem in lastEditedJournals)
                {
                    NormalIssue issue = _redmineDatabase.GetIssueByIssueId(journalItem.IssueId);
                    List<int> watchersRedmineIds = _redmineDatabase.GetWatchersIdList(issue.Id);

                    SendJournalToUser(issue.CreatorId, journalItem, issue);
                    if (issue.CreatorId != issue.AssignedTo)
                        SendJournalToUser(issue.AssignedTo, journalItem, issue);
                    foreach (int watcherRedmineId in watchersRedmineIds)
                    {
                        if (watcherRedmineId == issue.AssignedTo 
                            || watcherRedmineId == issue.CreatorId)
                        {
                            continue;
                        }
                        SendJournalToUser(watcherRedmineId, journalItem, issue);
                    }
                }

                lastIssuesUpdateCheckTime = DateTime.Now;
                Thread.Sleep(2500);
            }
        }

        private void SendIssueToUser(long redmineUserId, NormalIssue issue)
        {
            if (!TryGetTelegramUserId(redmineUserId, out long telegramId))
            {
                return;
            }

            if (issue.AssignedTo == redmineUserId)
            {
                _telegramBot.SendNewIssueToAssignedUser(telegramId, issue);
            }
            else
            {
                _telegramBot.SendNewIssueToWatcherOrCreator(telegramId, issue);
            }
        }

        private void SendJournalToUser(long redmineUserId, JournalItem journal, NormalIssue issue)
        {
            if (!TryGetTelegramUserId(redmineUserId, out long telegramId))
            {
                return;
            }
            bool isSendingToAssigned = issue.AssignedTo == redmineUserId;

            if (journal.IsComment)
            {
                if (isSendingToAssigned)
                {
                    _telegramBot.SendCommentNotificationToAssignedUser(telegramId, journal, issue);
                }
                else
                {
                    _telegramBot.SendCommentNotificationToWatcherOrCreator(telegramId, journal, issue);
                }
            }
            else if (journal.IsIssueStatusChange)
            {
                if (isSendingToAssigned)
                {
                    _telegramBot.SendStatusChangeNotificationToAssignedUser(telegramId, journal, issue);
                }
                else
                {
                    _telegramBot.SendStatusChangeNotificationToWatcherOrCreator(telegramId, journal, issue);
                }
            }
        }

        private bool TryGetTelegramUserId(long redmineId, out long telegramId)
        {
            if (_redmineDatabase.TryGetTelegramUsernameByRedmineId((int)redmineId, out string username)
                && _internalDatabase.TryGetUserTelegramIdByUsername(username, out long userId))
            {
                telegramId = userId;
                return true;
            }

            telegramId = 0;
            return false;
        }
    }
}
