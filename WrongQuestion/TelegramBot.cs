using System;
using System.Collections.Generic;
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

            RedmineTask redmineTask = new RedmineTask(123, Tracker.Defect, "dss",
                DateTime.Now, Status.New, "xyi", new List<Comment>() { new Comment(new RedmineUser(232, "pidor"), DateTime.Now, "pizda kicuk"),
                    new Comment(new RedmineUser(228, "zalupa"), DateTime.Now, "xui kicuk")});

            string comments = "---------------------------\n";
            foreach (var item in redmineTask.Comments)
            {
                comments += "Автор: " + item.Author.Name + '\n' + "Время: " + item.DateTime + '\n' + item.Content + '\n';
                comments += "---------------------------\n";
            }

            _bot.SendTextMessageAsync(msg.Chat.Id, "Важность: " + redmineTask.Tracker.ToString() + '\n' + "Заголовок: " + redmineTask.Topic + '\n' +
                "Время и дата: " + redmineTask.DateTime + '\n' + "Статус выполнения: " + redmineTask.Status.ToString() + '\n' + "Описание: " + redmineTask.Description +
                '\n' + "Комментарии:\n" + comments);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Добавить заметку", "11"),
                    InlineKeyboardButton.WithCallbackData("Поменять статус", "12"),
                }
            });
            if (msg.Text != null)
            {
                Console.WriteLine(msg.Text);
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Нажмите на кнопку:", replyMarkup: inlineKeyboard);
            }
        }
    }
}
