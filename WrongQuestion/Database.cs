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
            if (data.FieldCount == 0) 
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

        private void CreateTableUsers()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE Users
                (
                    RedmineUserId INTEGER NOT NULL,
                    TelegramChatId INTEGER NOT NULL
                );
            ";
            command.ExecuteNonQuery();
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
    }
}
