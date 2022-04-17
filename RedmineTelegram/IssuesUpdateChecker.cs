﻿using System;
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
        private readonly TelegramBot _telegramBot;

        public IssuesUpdateChecker(InternalDatabase internalDatabase, 
            TelegramBot telegramBot)
        {
            _internalDatabase = internalDatabase;
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
                List<JournalItem> lastEditedJournals = RedmineDatabase.LoadLastJournalsLine(lastIssuesUpdateCheckTime);
                List<Issue> lastCreatedIssues = RedmineDatabase.LoadLastCreatedIssues(lastIssuesUpdateCheckTime);

                foreach (Issue issue in lastCreatedIssues)
                {
                    List<long> issueRecipientsIds = RedmineDatabase.GetWatchersIdList(issue.Id);

                    issueRecipientsIds.Remove(issue.CreatorId);
                    issueRecipientsIds.RemoveAll(id => id == issue.AssignedTo);
                    issueRecipientsIds.Add(issue.AssignedTo);

                    foreach (int watcherRedmineId in issueRecipientsIds)
                    {
                        SendIssueToUser(watcherRedmineId, issue);
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

                    foreach (int watcherRedmineId in journalRecipientsIds)
                    {
                        SendJournalToUser(watcherRedmineId, journalItem, issue);
                    }
                }

                lastIssuesUpdateCheckTime = DateTime.Now;
                Thread.Sleep(2500);
            }
        }

        private void SendIssueToUser(long redmineUserId, Issue issue)
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
                _telegramBot.SendNewIssueToWatcher(telegramId, issue);
            }
        }

        private void SendJournalToUser(long redmineUserId, JournalItem journal, Issue issue)
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
                    if (redmineUserId == issue.CreatorId)
                    {
                        _telegramBot.SendCommentNotificationToWatcherOrCreator(telegramId, journal, issue);
                    }
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
                    if (redmineUserId == issue.CreatorId || issue.IsClosed)
                    {
                        _telegramBot.SendStatusChangeNotificationToWatcherOrCreator(telegramId, journal, issue);
                    }
                }
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
