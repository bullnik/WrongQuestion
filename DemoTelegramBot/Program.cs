using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace DemoTelegramBot
{
    class Program
    {
        private static TelegramBotClient _bot;
        //Here is the token for bot Jijoba @jijoba_bot:
        private static string Token => "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";

        public static void Main()
        {
            _bot = new TelegramBotClient(Token);
            _bot.OnMessage += OnMessageHandler;
            _bot.OnCallbackQuery += OnButtonClick;
            _bot.StartReceiving();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            string text = e.Message.Text;
            if (text == "/start")
            {
                string a = e.Message.From.Username;
                List<string> usernames = Data.GetAllUsers();
                if (usernames.Contains(e.Message.From.Username))
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id,
                        "Ваш аккаунт связан с Redmine");
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id,
                        "Ваш аккаунт не связан с Redmine");
                }
            }
        }

        private static async void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            var callbackData = e.CallbackQuery.Data;
            long taskId = long.Parse(callbackData[1..]);
            if (callbackData[0] == 'c')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст комментария.");
            }
            if (callbackData[0] == 'd')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст статуса.");
            }
        }
    }
}
