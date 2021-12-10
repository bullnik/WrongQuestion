using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using Microsoft.Data.Sqlite;

namespace WrongQuestion
{
    public class Database
    {
        private SqliteConnection _connection = new SqliteConnection("Data Source=usersdata.db");
        private DatabaseConverter _converter = new DatabaseConverter();

        public Database()
        {

        }

        public bool FindTaskById(int id, out RedmineTask task)
        {
            _connection.Open();

            var command = new SqliteCommand
            {
                Connection = _connection,

                CommandText = "SELECT * " +
                            "FROM Tasks " +
                            $"WHERE Id == {id};"
            };

            var data = command.ExecuteReader();

            if (data.FieldCount == 0) 
            {
                task = null;
                return false;
            }

            data.Read();
            int taskId = (int) (long) data.GetValue(0);
            DateTime dateTime = _converter.StringToDateTime((string)data.GetValue(1));
            Tracker tracker = _converter.StringToTracker((string) data.GetValue(2));
            string topic = (string) data.GetValue(3);
            Status status = _converter.StringToStatus((string) data.GetValue(4));
            string description = (string) data.GetValue(5);

            command = new SqliteCommand
            {
                Connection = _connection,

                CommandText = "SELECT * " +
                            "FROM Comments " +
                            $"WHERE TaskId == {id};"
            };

            List<Comment> comments = new List<Comment>();
            data = command.ExecuteReader();
            while (data.Read())
            {
                int userId = (int) (long) data.GetValue(1);
                DateTime commentDateTime = _converter.StringToDateTime((string)data.GetValue(2));
                string content = (string) data.GetValue(3);
                comments.Add(new Comment(new RedmineUser(userId, "govno"), commentDateTime, content));
            }

            task = new RedmineTask(taskId, tracker, topic, dateTime, status, description, comments);

            _connection.Close();
            return true;
        }

        public void InsertTask(RedmineTask task)
        {
            _connection.Open();

            new SqliteCommand
            {
                Connection = _connection,

                CommandText =
                "INSERT INTO Tasks (Id, DateTime, Tracker, Topic, Status, Description)" +
                $"VALUES ({task.Id}, '{_converter.DateTimeToString(task.DateTime)}', '{task.Tracker}', " +
                $"'{task.Topic}', '{task.Status}', '{task.Description}');"
            }
            .ExecuteNonQuery();

            foreach (Comment comment in task.Comments)
            {
                new SqliteCommand
                {
                    Connection = _connection,

                    CommandText =
                    "INSERT INTO Comments (TaskId, UserId, DateTime, Content)" +
                    $"VALUES ({task.Id}, {comment.Author.Id}, '{_converter.DateTimeToString(comment.DateTime)}', " +
                    $"'{comment.Content}');"
                }
                .ExecuteNonQuery();
            }

            _connection.Close();
        }

        public void CreateTableTasks()
        {
            _connection.Open();

            SqliteCommand command = new SqliteCommand
            {
                Connection = _connection,

                CommandText =
                "CREATE TABLE Tasks" +
                "(" +
                "Id INTEGER NOT NULL," +
                "DateTime DATETIME NOT NULL," +
                "Tracker TEXT NOT NULL," +
                "Topic TEXT NOT NULL," +
                "Status TEXT NOT NULL," +
                "Description TEXT NOT NULL" +
                ");"
            };

            command.ExecuteNonQuery();

            _connection.Close();
        }

        public void CreateTableComments()
        {
            _connection.Open();

            SqliteCommand command = new SqliteCommand
            {
                Connection = _connection,

                CommandText =
                "CREATE TABLE Comments" +
                "(" +
                "TaskId INTEGER NOT NULL," +
                "UserId INTEGER NOT NULL," +
                "DateTime DATETIME NOT NULL," +
                "Content TEXT NOT NULL" +
                ");"
            };

            command.ExecuteNonQuery();

            _connection.Close();
        }
    }
}
