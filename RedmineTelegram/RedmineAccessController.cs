using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class RedmineAccessController
    {
        private readonly InternalDatabase _internalDatabase;

        public RedmineAccessController(InternalDatabase internalDatabase)
        {
            _internalDatabase = internalDatabase;
        }

        public bool VerifyRedmineUserByTelegramIdAndUsername(long telegramUserId, string telegramUsername, out long redmineId)
        {
            if (RedmineDatabase.TryGetRedmineUserIdByTelegram(telegramUsername, out redmineId))
            {
                _internalDatabase.InsertUserToDatabaseIfNotExists(telegramUserId, telegramUsername);
                return true;
            }

            ResetExpectedActionAndIssueByTelegramUserId(telegramUserId);
            return false;
        }

        public Tuple<ExpectedAction, long> GetExpectedActionAndChangedIssueByUserId(long telegramUserId)
        {
            return _internalDatabase.GetExpectedActionAndChangedIssueByTelegramUserId(telegramUserId);
        }

        public void ChangeExpectedActionAndIssueByTelegramUserId(ExpectedAction expectedAction, long issueId, long telegramUserId)
        {
            _internalDatabase.ChangeIssueAndExpectedActionByTelegramUserId(expectedAction, issueId, telegramUserId);
        }

        public void ResetExpectedActionAndIssueByTelegramUserId(long telegramUserId)
        {
            ChangeExpectedActionAndIssueByTelegramUserId(ExpectedAction.Nothing, 0, telegramUserId);
        }

        public static List<Issue> GetUserIssuesByRedmineUserId(long redmineUserId)
        {
            return RedmineDatabase.GetUserIssues(redmineUserId);
        }

        public static List<string> GetStatusesList()
        {
            return RedmineDatabase.GetStatusesList();
        }

        public static Issue GetIssueByIssueId(long issueId)
        {
            return RedmineDatabase.GetIssueByIssueId(issueId);
        }

        public static bool AddLaborCost(long issueId, float hours, string comment, long redmineUserId)
        {
            if (RedmineDatabase.CheckIsUserAssignedToIssue(issueId, redmineUserId) == false)
                return false;
            return RedmineDatabase.AddLaborCost(issueId, hours, comment, redmineUserId);
        }

        public static bool AddComment(long issueId, string comment, long redmineUserId)
        {
            if (RedmineDatabase.CheckIsUserAssignedToIssue(issueId, redmineUserId) == false)
                return false;
            return RedmineDatabase.AddComment(issueId, comment, redmineUserId);
        }

        public static bool ChangeStatus(long issueId, string statusName, long redmineUserId)
        {
            if (RedmineDatabase.CheckIsUserAssignedToIssue(issueId, redmineUserId) == false)
                return false;
            long statusId = RedmineDatabase.GetStatusIdByName(statusName);
            return RedmineDatabase.ChangeStatus(issueId, statusId, redmineUserId);
        }
    }
}
