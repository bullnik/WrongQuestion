using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;

namespace WrongQuestion
{
    class Program
    {
        //Here is the token for bot Jijoba @jijoba_bot:
        //2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0
        private static string Token => "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";

        private static TelegramBotClient Bot;

        static void Main()
        {
            Bot = new TelegramBotClient(Token);

            Bot.StartReceiving();

            Bot.OnMessage += OnMessageHandler;
            Console.ReadKey();

            Bot.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            if (msg.Text != null)
            {
                Console.WriteLine(msg.Text);
                await Bot.SendTextMessageAsync(msg.Chat.Id, "Ты написал: " + msg.Text + ". Пошел нахуй теперь.");
            }
        }
    }
}
