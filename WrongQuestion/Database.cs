using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace WrongQuestion
{
    public class Database
    {
        private readonly string _connectionString = "Data Source=usersdata.db";
        private readonly DatabaseConverter _converter = new DatabaseConverter();

        public Database()
        {

        }

        public bool TryFindTaskById(long id, out RedmineTask task)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Tasks
                WHERE Id == $id;
            ";
            command.Parameters.AddWithValue("$id", id);
            var data = command.ExecuteReader();
            if (!data.HasRows) 
            {
                task = null;
                return false;
            }
            data.Read();
            long taskId = data.GetInt32(0);
            DateTime dateTime = data.GetDateTime(1);
            string tracker = data.GetString(2);
            string topic = data.GetString(3);
            string status = data.GetString(4);
            string description = data.GetString(5);

            command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Comments
                WHERE TaskId == $id;
            ";
            command.Parameters.AddWithValue("$id", id);
            List<Comment> comments = new List<Comment>();
            data = command.ExecuteReader();
            while (data.Read())
            {
                long userId = data.GetInt32(1);
                DateTime commentDateTime = data.GetDateTime(2);
                string content = data.GetString(3);
                comments.Add(new Comment(new RedmineUser(userId, "govno"), commentDateTime, content));
            }
            task = new RedmineTask(taskId, tracker, topic, dateTime, status, description, comments);
            return true;
        }

        public void InsertOrUpdateTask(RedmineTask task)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                DELETE FROM Tasks
                WHERE Id == $id
            ";
            command.Parameters.AddWithValue("$id", task.Id);
            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText =
            @"
                DELETE FROM Comments
                WHERE TaskId == $id
            ";
            command.Parameters.AddWithValue("$id", task.Id);
            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO Tasks (Id, DateTime, Tracker, Topic, Status, Description)
                VALUES ($id, $dateTime, $tracker, $topic, $status, $desc);
            ";
            command.Parameters.AddWithValue("$id", task.Id);
            command.Parameters.AddWithValue("$dateTime", _converter.DateTimeToString(task.DateTime));
            command.Parameters.AddWithValue("$tracker", task.Tracker);
            command.Parameters.AddWithValue("$topic", task.Topic);
            command.Parameters.AddWithValue("$status", task.Status);
            command.Parameters.AddWithValue("$desc", task.Description);
            command.ExecuteNonQuery();

            foreach (Comment comment in task.Comments)
            {
                command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Comments (TaskId, UserId, DateTime, Content)
                    VALUES ($id, $authorId, $dateTime, $content);
                ";
                command.Parameters.AddWithValue("$id", task.Id);
                command.Parameters.AddWithValue("$authorId", comment.Author.Id);
                command.Parameters.AddWithValue("$dateTime", _converter.DateTimeToString(comment.DateTime));
                command.Parameters.AddWithValue("$content", comment.Content);
                command.ExecuteNonQuery();
            }
        }

        public bool TryFindUserByChatId(long chatId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Users
                WHERE TelegramChatId == $id;
            ";
            command.Parameters.AddWithValue("$id", chatId);
            var data = command.ExecuteReader();
            data.Read();
            return data.FieldCount > 0;
        }

        public bool TryFindUserByTelegramUsername(string telegramLogin)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Users
                WHERE TelegramUsername == $login;
            ";
            command.Parameters.AddWithValue("$login", telegramLogin);
            var data = command.ExecuteReader();
            data.Read();
            return data.FieldCount > 0;
        }

        public bool ChangeTelegramChatStatus(long chatId, TelegramChatStatus status)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT LatestChangedTaskId
                UPDATE Users
                SET TelegramChatStatus = $status
                WHERE TelegramChatId == $id;
            ";
            command.Parameters.AddWithValue("$id", chatId);
            command.Parameters.AddWithValue("$status", (long) status);
            command.ExecuteNonQuery();
            return true;
        }

        private void CreateTableTasks()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE OR REPLACE TABLE Tasks
                (
                    Id INTEGER NOT NULL,
                    DateTime DATETIME NOT NULL,
                    Tracker TEXT NOT NULL,
                    Topic TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    Description TEXT NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }

        private void CreateTableComments()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE Comments
                (
                    TaskId INTEGER NOT NULL,
                    UserId INTEGER NOT NULL,
                    DateTime DATETIME NOT NULL,
                    Content TEXT NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }

        public void InsertUser(long chatId, string redmineUsername = "", 
                        string telegramUsername = "", int redmineId = 0)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO Users (RedmineUserId, RedmineUsername, TelegramChatId, 
                    TelegramUsername, TelegramChatStatus)
                VALUES ($redId, $redUN, $telCI, $telUN, $telCS);
            ";
            command.Parameters.AddWithValue("$redId", redmineId);
            command.Parameters.AddWithValue("$redUN", redmineUsername);
            command.Parameters.AddWithValue("$telCI", chatId);
            command.Parameters.AddWithValue("$telUN", telegramUsername);
            command.Parameters.AddWithValue("$telCS", 1);
            command.ExecuteNonQuery();
        }

        private void CreateTableUsers()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE Users
                (
                    RedmineUserId INTEGER,
                    RedmineUsername TEXT,
                    TelegramChatId INTEGER NOT NULL,
                    TelegramUsername TEXT, 
                    TelegramChatStatus INTEGER NOT NULL,
                    LatestChangedTaskId INTEGER
                );
            ";
            command.ExecuteNonQuery();
        }

        public bool TryGetLatestChangedTaskIdByChatId(long chatId, out long taskId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT LatestChangedTaskId
                FROM Users
                WHERE TelegramChatId == $id;
            ";
            command.Parameters.AddWithValue("$id", chatId);
            var data = command.ExecuteReader();
            data.Read();
            if (data.FieldCount > 0)
            {
                taskId = data.GetInt32(0);
                return true;
            }
            else
            {
                taskId = 0;
                return false;
            }
        }

        private void CreateTableMessages()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE Messages
                (
                    ChatId INTEGER NOT NULL,
                    TaskId INTEGER NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }

        public bool isUserLogged(string telegramLogin = "", long telegramId = 0)
        {
            return TryFindUserByChatId(telegramId) || TryFindUserByTelegramUsername(telegramLogin);
        }

        public TelegramChatStatus GetTelegramChatStatus(long chatId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT TelegramChatStatus
                FROM Users
                WHERE TelegramChatId == $id;
            ";
            command.Parameters.AddWithValue("$id", chatId);
            var data = command.ExecuteReader();
            data.Read();
            if (data.HasRows)
            {
                return (TelegramChatStatus) data.GetInt32(0);
            }
            else
            {
                return 0;
            }
        }
    }
}
