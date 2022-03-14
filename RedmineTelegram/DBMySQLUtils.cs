using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class DBMySQLUtils
    {
        public static MySqlConnection GetDBConnection(string host, int port, 
            string database, string username, string password)
        {
            string connectionString = "Server=" + host + ";Database=" + database
                + ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new(connectionString);

            return conn;
        }
    }
}
