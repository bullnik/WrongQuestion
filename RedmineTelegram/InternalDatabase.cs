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
            return false;
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
    }
}
