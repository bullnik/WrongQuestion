using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class IssuesUpdatesChecker
    {
        private readonly InternalDatabase _internalDatabase;
        private readonly TelegramBot _telegramBot;
        private CancellationToken _cancellationToken;

        public IssuesUpdatesChecker(InternalDatabase internalDatabase, 
            TelegramBot telegramBot)
        {
            _internalDatabase = internalDatabase;
            _telegramBot = telegramBot;
        }

        public void StartChecking(CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            Task.Run(() => Checking(), cancellationToken);
        }

        private void Checking()
        {
            DateTime lastIssuesUpdateCheckTime = DateTime.Now;

            while (!_cancellationToken.IsCancellationRequested)
            {
                List<JournalItem> lastEditedJournals = RedmineDatabase.LoadLastJournalsLine(lastIssuesUpdateCheckTime);
                List<Issue> lastCreatedIssues = RedmineDatabase.LoadLastCreatedIssues(lastIssuesUpdateCheckTime);

                foreach (Issue issue in lastCreatedIssues)
                {
                    List<long> issueRecipientsIds = RedmineDatabase.GetWatchersIdList(issue.Id);

                    issueRecipientsIds.Remove(issue.CreatorId);
                    issueRecipientsIds.RemoveAll(id => id == issue.AssignedTo);
                    issueRecipientsIds.Add(issue.AssignedTo);

                    foreach (int recipientRedmineId in issueRecipientsIds)
                    {
                        SendNewIssueNotificationToUser(recipientRedmineId, issue);
                    }
                }

                foreach (JournalItem journalItem in lastEditedJournals)
                {
                    Issue issue = RedmineDatabase.GetIssueByIssueId(journalItem.IssueId);
                    List<long> journalRecipientsIds = RedmineDatabase.GetWatchersIdList(issue.Id);

                    journalRecipientsIds.RemoveAll(id => id == issue.CreatorId);
                    journalRecipientsIds.Add(issue.CreatorId);
                    journalRecipientsIds.RemoveAll(id => id == issue.AssignedTo);
                    journalRecipientsIds.Add(issue.AssignedTo);
                    journalRecipientsIds.RemoveAll(id => id == journalItem.UserId);

                    foreach (int recipientRedmineId in journalRecipientsIds)
                    {
                        SendNewJournalNotificationToUser(recipientRedmineId, journalItem, issue);
                    }
                }

                lastIssuesUpdateCheckTime = DateTime.Now;
                Thread.Sleep(2500);
            }
        }

        private void SendNewIssueNotificationToUser(long redmineUserId, Issue issue)
        {
            if (!TryGetTelegramUserId(redmineUserId, out long telegramId))
            {
                return;
            }

            UserStatus userStatus = GetUserStatus(issue, redmineUserId);
            _telegramBot.SendNewIssueNotification(telegramId, issue, userStatus);
        }

        private void SendNewJournalNotificationToUser(long redmineUserId, JournalItem journal, Issue issue)
        {
            if (!TryGetTelegramUserId(redmineUserId, out long telegramId))
            {
                return;
            }

            UserStatus userStatus = GetUserStatus(issue, redmineUserId);
            if (journal.IsComment && userStatus != UserStatus.IssueWatcher
                || (journal.IsIssueStatusChange && userStatus != UserStatus.IssueWatcher)
                || (journal.IsIssueStatusChange && issue.IsClosed))
            {
                _telegramBot.SendCommentNotification(telegramId, journal, issue, userStatus);
            }
        }

        private static UserStatus GetUserStatus(Issue issue, long redmineUserId)
        {
            if (redmineUserId == issue.CreatorId)
            {
                return UserStatus.IssueCreator;
            }
            else if (redmineUserId == issue.AssignedTo)
            {
                return UserStatus.AssignedToIssue;
            }
            else
            {
                return UserStatus.IssueWatcher;
            }
        }

        private bool TryGetTelegramUserId(long redmineId, out long telegramId)
        {
            if (RedmineDatabase.TryGetTelegramUsernameByRedmineId((int)redmineId, out string username)
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
