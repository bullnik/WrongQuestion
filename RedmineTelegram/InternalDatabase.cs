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

        }

        public Tuple<ExpectedAction, long> GetChangedIssueAndExpectedActionByUserId(long userId)
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
            int a = 0;
            long b = 0;
            if (data.HasRows)
            {
                a = data.GetInt32(0);
                b = data.GetInt64(1);
            }
            connection.Close();
            return new Tuple<ExpectedAction, long>((ExpectedAction)a, b);
        }

        public void ChangeIssueAndExpectedActionByUserId(ExpectedAction action, long issueId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users
                SET ExpectedAction = $action, ChangedIssueId = $id
                WHERE TelegramUserId == 842190162;
            ";
            command.Parameters.AddWithValue("$action", action);
            command.Parameters.AddWithValue("$id", issueId);
            var data = command.ExecuteNonQuery();
            connection.Close();
        }

        public void InsertUserToDatabase(long telegramUserId, string username)
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

        public bool TryGetIssueById(int id, out Issue issue)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * 
                FROM Issues
                WHERE Id == $id
            ";
            command.Parameters.AddWithValue("$id", id);
            var data = command.ExecuteReader();
            int a = 0;
            int b = 0;
            int da = 0;
            int sa = 0;
            data.Read();
            if (data.HasRows)
            {
                a = data.GetInt32(0);
                b = data.GetInt32(1);
                da = data.GetInt32(2);
                sa = data.GetInt32(3);
                issue = new Issue(a, da, b > 0, sa);
                return true;
            }
            issue = new Issue(a, da, b > 0, sa);
            connection.Close();
            return false;
        }

        private SqliteDataReader ExecuteReaderCommand(string commandText)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            var data = command.ExecuteReader();
            connection.Close();
            return data;
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
                CREATE TABLE Issues
                (
                    Id INTEGER,
                    Closed INTEGER,
                    AssignedTo INTEGER,
                    Status INTEGER
                );
            ";

        private readonly string CreateTableUsersCommandText =
            @"
                CREATE TABLE Users 
                (
                    TelegramUserId INTEGER,
                    TelegramUsername INTEGER,
                    ExpectedAction INTEGER,
                    ChangedIssueId INTEGER
                );
            ";
    }
}
