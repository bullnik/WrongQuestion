using System;
using System.Threading;
using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class Program
    {
        static void Main(string[] args)
        {
            InternalDatabase internalDatabase = new();
            RedmineDatabase redmineDatabase = new();

            TelegramBot bot = new(redmineDatabase);
            bot.StartReceiving();

            IssuesUpdateChecker issuesUpdateChecker = new(internalDatabase, redmineDatabase);
            issuesUpdateChecker.StartChecking();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
