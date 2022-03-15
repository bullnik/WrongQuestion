using System;
using System.Collections.Generic;

namespace RedmineTelegram
{
    public sealed class RedmineDatabase
    {
        public List<NormalIssue> GetUserIssues(long userId)
        {
            throw new Exception();
        }

        public List<Issue> LoadLastEditedIssues(int count)
        {
            throw new Exception();
        }

        public bool TryGetTelegramUsernameByRedmineId(int assignedTo, out string username)
        {
            throw new Exception();
        }

        public List<string> GetStatusesList()
        {
            throw new Exception();
        }

        public int GetStatusIdByName(string statusName)
        {
            throw new Exception();
        }

        public bool TryGetRedmineUserIdByTelegram(string telegramUsername, out long redmineUserId)
        {
            throw new Exception();
        }

        public NormalIssue GetNormalIssue(int issueId)
        {
            throw new Exception();
        }

        public bool ChangeIssueStatus(long issueId, int statusId)
        {
            throw new Exception();
        }

        public void ChangeLaborCost(long issueId, int laborCost)
        {
            throw new Exception();
        }
    }
}