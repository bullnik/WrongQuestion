using System;
using System.Threading;
using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class Program
    {
        static void Main()
        {
            InternalDatabase internalDatabase = new();
            RedmineDatabase redmineDatabase = new();

            TelegramBot telegramBot = new(redmineDatabase, internalDatabase);
            telegramBot.StartReceiving();

            IssuesUpdateChecker issuesUpdateChecker = new(internalDatabase, redmineDatabase, telegramBot);
            issuesUpdateChecker.StartChecking();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
