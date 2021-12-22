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
            _bot.OnCallbackQuery += OnButtonClick;
            Console.ReadKey();
            _bot.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            if(e.Message.Text == "/start")
            {
                if(new Database().isUserLogged(e.Message.From.Username))
                {
                    SendTasksForUser(e);
                }
                else
                {
                    _bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите логин и пароль одной строкой через пробел");


                }
            }
            else
            {

            }
        }

        private static async void Authorization(MessageEventArgs e)
        {
            if (new Database().isUserLogged(e.Message.From.Username))
            {
                SendTasksForUser(e);
            }
            else
            {
                _bot.SendTextMessageAsync(e.Message.Chat.Id, "Вы ввели неверный пароль или не зарегестрированы в системе, введите пароль");
                Authorization(e);
            }
        }

        private static async void SendTasksForUser(MessageEventArgs e)
        {
            var msg = e.Message;
            RedmineTask redmineTask = new RedmineTask(123, "sdfds", "dss",
                DateTime.Now, "sadad", "xyi", new List<Comment>() { new Comment(new RedmineUser(232, "pidor"), DateTime.Now, "pizda kicuk"),
                    new Comment(new RedmineUser(228, "zalupa"), DateTime.Now, "xui kicuk")});

            RedmineTask redmineTask2 = new RedmineTask(124, "sdf2ds", "ds2s",
                DateTime.Now, "sada2d", "xy2i", new List<Comment>() { new Comment(new RedmineUser(2322, "p2idor"), DateTime.Now, "pizda kic2uk"),
                    new Comment(new RedmineUser(228, "zalu2pa"), DateTime.Now, "xui k2icuk")});

            List<RedmineTask> tasks = new List<RedmineTask>();
            tasks.Add(redmineTask);
            tasks.Add(redmineTask2);


            foreach (var task in tasks)
            {
                var editing = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Добавить заметку", "c" + task.Id.ToString()),
                        InlineKeyboardButton.WithCallbackData("Поменять статус",  "d" + task.Id.ToString()),
                    }
                });

                string comments = "---------------------------\n";
                foreach (var comment in task.Comments)
                {
                    comments += "Автор: " + comment.Author.Name + '\n' + "Время: " + task.DateTime + '\n' + comment.Content + '\n';
                    comments += "---------------------------\n";
                }
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Важность: " + task.Tracker.ToString() + '\n' + "Заголовок: " + task.Topic + '\n' +
                "Время и дата: " + task.DateTime + '\n' + "Статус выполнения: " + task.Status.ToString() + '\n' + "Описание: " + task.Description +
                '\n' + "Комментарии:\n" + comments, replyMarkup: editing);
            }
        }
        private static async void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            var d = e.CallbackQuery.Data;
            if(d[0] == 'c')
            {
                _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст заметки");
                Console.WriteLine("xui sovdaem zametku");
            }
            if (d[0] == 'd')
            {
                _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст статуса");
                Console.WriteLine("jopa menyem status");
            }
        }
    }
}
