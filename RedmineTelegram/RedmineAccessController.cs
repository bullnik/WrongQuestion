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

            _internalDatabase.RemoveUserFromDatabase(telegramUserId);
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

        public void AddLaborCost(long issueId, double hours, string comment, string telegramUsername)
        {
            _redmineDatabase.ChangeLaborCost(issueId, hours, comment, telegramUsername);
        }
    }
}
