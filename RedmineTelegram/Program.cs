using System;
using System.Threading;
using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class Program
    {
        static void Main(string[] args)
        {
            TestConnection();
        }

        static void TestConnection()
        {
            Console.WriteLine("Getting Connection ...");
            MySqlConnection conn = DBUtils.GetDBConnection();

            try
            {
                Console.WriteLine("Openning Connection ...");
                conn.Open();
                Console.WriteLine("Connection successful!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            Console.Read();
        }
    }
}
