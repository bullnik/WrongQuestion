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

            RedmineAccessController redmineAccessController = new(internalDatabase);

            TelegramBot telegramBot = new(redmineAccessController);
            telegramBot.StartReceiving();

            IssuesUpdateChecker issuesUpdateChecker = new(internalDatabase, telegramBot);
            issuesUpdateChecker.StartChecking();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
