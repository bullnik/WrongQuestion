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
            ExecuteNonQueryCommand(CreateTableSavedTasksCommandText);
        }

        private SqliteDataReader ExecuteReaderCommand(string commandText)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            var data = command.ExecuteReader();
            return data;
        }

        private void ExecuteNonQueryCommand(string commandText)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }

        private readonly string CreateTableSavedTasksCommandText = 
            @"
                CREATE TABLE SavedTasks
                (
                    id INTEGER,
                    RedmineUsername TEXT,
                    TelegramChatId INTEGER NOT NULL,
                    TelegramUsername TEXT, 
                    TelegramChatStatus INTEGER NOT NULL,
                    LatestChangedTaskId INTEGER
                );
            ";
    }
}
