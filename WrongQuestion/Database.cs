using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace WrongQuestion
{
    public class Database
    {
        private SqliteConnection _connection;

        public Database()
        {
            _connection = new SqliteConnection("Data Source=usersdata.db");
        }

        public bool FindTaskById(int id)
        {
            return false;
        }

        public void InsertTask(RedmineTask task)
        {
            _connection.Open();

            string format = "yyyy-MM-dd HH:mm:ss";

            string text = "INSERT INTO Tasks (Id, DateTime, Tracker, Topic, Status, Description)" +
                $"VALUES ({task.Id}, '{task.DateTime.ToString(format)}', '{task.Tracker}', " +
                $"'{task.Topic}', '{task.Status}', '{task.Description}');";

            new SqliteCommand
            {
                Connection = _connection,

                CommandText =
                "INSERT INTO Tasks (Id, DateTime, Tracker, Topic, Status, Description)" +
                $"VALUES ({task.Id}, '{task.DateTime.ToString(format)}', '{task.Tracker}', " +
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
                    $"VALUES ({task.Id}, {comment.Author.Id}, '{comment.DateTime.ToString(format)}', " +
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
