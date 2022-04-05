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
        private readonly RedmineDatabase _redmineDatabase;

        public RedmineAccessController(InternalDatabase internalDatabase, RedmineDatabase redmineDatabase)
        {
            _internalDatabase = internalDatabase;
            _redmineDatabase = redmineDatabase;
        }

        public bool VerifyRedmineUserByTelegramIdAndUsername(long telegramUserId, string telegramUsername, out long redmineId)
        {
            if (_redmineDatabase.TryGetRedmineUserIdByTelegram(telegramUsername, out redmineId))
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

        public List<NormalIssue> GetUserIssuesByRedmineUserId(long redmineUserId)
        {
            return _redmineDatabase.GetUserIssues(redmineUserId);
        }

        public List<string> GetStatusesList()
        {
            return _redmineDatabase.GetStatusesList();
        }

        public NormalIssue GetIssueByIssueId(long issueId)
        {
            return _redmineDatabase.GetIssueByIssueId(issueId);
        }

        public bool AddLaborCost(long issueId, double hours, string comment, long redmineUserId)
        {
            return _redmineDatabase.AddLaborCost(issueId, hours, comment, redmineUserId);
        }

        public bool AddComment(long issueId, string comment, long redmineUserId)
        {
            return _redmineDatabase.AddComment(issueId, comment, redmineUserId);
        }

        public bool ChangeStatus(long issueId, string statusName, long redmineUserId)
        {
            long statusId = _redmineDatabase.GetStatusIdByName(statusName);
            return _redmineDatabase.ChangeStatus(issueId, statusId, redmineUserId);
        }
    }
}
