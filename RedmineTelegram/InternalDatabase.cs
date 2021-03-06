using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace RedmineTelegram
{
    public sealed class InternalDatabase
    {
        private readonly string _connectionString = "Data Source=usersdata.db";

        public InternalDatabase()
        {
            ExecuteNonQueryCommand(CreateTableIssuesCommandText);
            ExecuteNonQueryCommand(CreateTableUsersCommandText);
        }

        public Tuple<ExpectedAction, long> GetExpectedActionAndChangedIssueByTelegramUserId(long userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ExpectedAction, ChangedIssueId 
                FROM Users
                WHERE TelegramUserId == $id
            ";
            command.Parameters.AddWithValue("$id", userId);
            var data = command.ExecuteReader();
            data.Read();
            int expectedAction = 0;
            long issueId = 0;
            if (data.HasRows)
            {
                expectedAction = data.GetInt32(0);
                issueId = data.GetInt64(1);
            }
            connection.Close();
            return new Tuple<ExpectedAction, long>((ExpectedAction)expectedAction, issueId);
        }

        public void ChangeIssueAndExpectedActionByTelegramUserId(ExpectedAction action, long issueId, long userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users
                SET ExpectedAction = $action, ChangedIssueId = $issueId
                WHERE TelegramUserId == $userId;
            ";
            command.Parameters.AddWithValue("$action", action);
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$issueId", issueId);
            var data = command.ExecuteNonQuery();
            connection.Close();
        }

        public void RemoveUserFromDatabase(long telegramUserId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                    DELETE FROM Users WHERE TelegramUserId == $tid
                ";
            command.Parameters.AddWithValue("$tid", telegramUserId);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void InsertUserToDatabaseIfNotExists(long telegramUserId, string username)
        {
            if (IsUserInDatabase(telegramUserId))
            {
                return;
            }
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = 
                @"
                    INSERT INTO Users (TelegramUserId, TelegramUsername, ExpectedAction, ChangedIssueId)
                        VALUES ($tid, $username, 0, 0)
                ";
            command.Parameters.AddWithValue("$tid", telegramUserId);
            command.Parameters.AddWithValue("$username", username);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public bool IsUserInDatabase(long userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = 
                @"
                    SELECT * FROM Users WHERE TelegramUserId == $id
                ";
            command.Parameters.AddWithValue("$id", userId);
            var data = command.ExecuteReader();
            data.Read();
            if (data.HasRows)
            {
                connection.Close();
                return true;
            }
            connection.Close();
            return false;
        }

        public bool TryGetUserTelegramIdByUsername(string telegramUsername, out long telegramUserId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
                    SELECT TelegramUserId FROM Users WHERE TelegramUsername == $id
                ";
            command.Parameters.AddWithValue("$id", telegramUsername);
            var data = command.ExecuteReader();
            data.Read();
            telegramUserId = 0;
            if (data.HasRows)
            {
                telegramUserId = data.GetInt64(0);
                connection.Close();
                return true;
            }
            connection.Close();
            return false;
        }

        private void ExecuteNonQueryCommand(string commandText)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
            connection.Close();
        }

        private readonly string CreateTableIssuesCommandText =
            @"
                CREATE TABLE IF NOT EXISTS Issues
                (
                    Id INTEGER,
                    Closed INTEGER,
                    AssignedTo INTEGER,
                    Status INTEGER
                );
            ";

        private readonly string CreateTableUsersCommandText =
            @"
                CREATE TABLE IF NOT EXISTS Users 
                (
                    TelegramUserId INTEGER,
                    TelegramUsername INTEGER,
                    ExpectedAction INTEGER,
                    ChangedIssueId INTEGER
                );
            ";
    }
}
