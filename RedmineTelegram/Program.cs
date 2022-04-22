using System;
using System.Threading;
using MySql.Data.MySqlClient;

namespace RedmineTelegram
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(DateTime.Now.ToString() + " Starting program ");

            string configurationFilePath = "configuration.json";
            Console.WriteLine("Loading configuration from " + configurationFilePath);
            if (!Configuration.TryGetFromJson(configurationFilePath, out Configuration config))
            {
                Console.WriteLine("Cannot read config file, loading default");
                config = new();
                config.WriteToJson(configurationFilePath);
            }

            Console.WriteLine("Loading internal database");
            InternalDatabase internalDatabase = new();
            RedmineAccessController redmineAccessController = new(internalDatabase);

            Console.WriteLine("Connecting to redmine database:");
            Console.WriteLine("Host - " + config.RedmineDatabaseHost);
            Console.WriteLine("Port - " + config.RedmineDatabasePort);
            Console.WriteLine("Database - " + config.RedmineDatabaseName);
            Console.WriteLine("User - " + config.RedmineDatabaseUsername);
            Console.WriteLine("Password - " + config.RedmineDatabasePassword);
            if (!RedmineDatabase.TryInitialize(config))
            {
                Console.WriteLine("Cannot connect to redmine database");
                Console.WriteLine("Check the connection data in config file");
                Console.ReadKey();
                return;
            }

            CancellationTokenSource cancellationTokenSource = new();

            Console.WriteLine("Initializing telegram bot");
            TelegramBot telegramBot = new(redmineAccessController, config);
            telegramBot.StartReceiving(cancellationTokenSource.Token);

            IssuesUpdatesChecker issuesUpdateChecker = new(internalDatabase, telegramBot);
            issuesUpdateChecker.StartChecking(cancellationTokenSource.Token);

            Console.WriteLine("Program started " + DateTime.Now.ToString());

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
