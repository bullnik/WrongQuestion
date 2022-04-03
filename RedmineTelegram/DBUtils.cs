using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "localhost";
            int port = 3307;
            string database = "";
            string username = "admindb";
            string password = "password123";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

    }
}
