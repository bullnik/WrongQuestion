using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "4.tcp.ngrok.io";
            int port = 17211;
            string database = "";
            string username = "admindb";
            string password = "password123";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

    }
}
