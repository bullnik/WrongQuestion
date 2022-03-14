using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "192.168.205.130";
            int port = 3306;
            string database = "simplehr";
            string username = "root";
            string password = "1234";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

    }
}
