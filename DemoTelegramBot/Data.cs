using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoTelegramBot
{
    public class Data
    {
        private readonly static string _connectionString = "Data Source=usersdata.db";

        public static List<string> GetAllUsers()
        {
            var data = ExecuteReaderCommand(SelectAllUsernames);
            List<string> usernames = new();
            while (data.HasRows)
            {
                usernames.Add(data.GetString(0));
                data.NextResult();
            }
            return usernames;
        }

        private static SqliteDataReader ExecuteReaderCommand(string commandText)
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

        private static readonly string SelectAllUsernames =
            @"
                SELECT u.TelegramUsername 
                FROM Users u;
            ";
    }
}
