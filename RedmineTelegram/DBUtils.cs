using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "0.tcp.ngrok.io";
            int port = 16516;
            string database = "";
            string username = "admindb";
            string password = "password123";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

    }
}
