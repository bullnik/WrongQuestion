using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace WrongQuestion
{
    class TelegramBot
    {
        private static TelegramBotClient _bot;
        //Here is the token for bot Jijoba @jijoba_bot:
        private static string Token => "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";

        public void Run()
        {
            _bot = new TelegramBotClient(Token);
            _bot.StartReceiving();
            _bot.OnMessage += OnMessageHandler;
            Console.ReadKey();
            _bot.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            if (msg.Text != null)
            {
                Console.WriteLine(msg.Text);
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Ты написал: " + msg.Text + ". Пошел нахуй теперь.");
            }
        }
    }
}
