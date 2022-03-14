using System;
using System.Threading;
using MySql.Data.MySqlClient;
using TelegramBot;

namespace RedmineTelegram
{
    class Program
    {
        static void Main(string[] args)
        {
            TelegramBot bot = new();
            RedmineDatabase redmine = new();
            bot.StartReceiving();

            while (true)
            {
                Thread.Sleep(10000);
                redmine.LoadLastEditedTasks();

            }
        }

        void TestConnection()
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
