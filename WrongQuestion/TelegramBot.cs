using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace WrongQuestion
{
    class TelegramBot
    {
        private TelegramBotClient _bot;
        private Database _database;
        private RedmineConnector _redmine;
        //Here is the token for bot Jijoba @jijoba_bot:
        private string Token => "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";

        public void Run()
        {
            _bot = new TelegramBotClient(Token);
            _database = new Database();
            _redmine = new RedmineConnector();
            _bot.OnMessage += OnMessageHandler;
            _bot.OnCallbackQuery += OnButtonClick;
            _bot.StartReceiving();
        }

        public void Stop()
        {
            if (_bot.IsReceiving)
            {
                _bot.StopReceiving();
            }
        }

        private async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            TelegramChatStatus status = _database.GetTelegramChatStatus(e.Message.Chat.Id);

            if (e.Message.Text == "/start")
            {
                if (status > 0)
                {
                    SendTasksForUser(e);
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, 
                        "Ваш аккаунт не связан с Redmine, для соединения введите логин от Redmine.");
                }
            }
            else if (status == TelegramChatStatus.Unauthorized)
            {
                if (_redmine.CheckLogin(e.Message.From.Username, e.Message.Text))
                {
                    _database.InsertUser(e.Message.Chat.Id);
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, 
                        "Вы ввели неверный логин, попробуйте снова.");
                }
                
            }
            else if (status == TelegramChatStatus.AuthorizedAndWaitingForChangeStatus)
            {
                if (_database.TryGetLatestChangedTaskIdByChatId(e.Message.Chat.Id, out long taskId))
                {
                    if (_redmine.ChangeStatus(taskId, e.Message.Text))
                    {
                        await _bot.SendTextMessageAsync(e.Message.Chat.Id, 
                            "Статус изменён.");
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(e.Message.Chat.Id,
                            "Не удалось изменить статус.");
                    }
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id,
                            "Не удалось изменить статус.");
                }
            }
            else if (status == TelegramChatStatus.AuthorizedAndWaitingForComment)
            {
                if (_database.TryGetLatestChangedTaskIdByChatId(e.Message.Chat.Id, out long taskId))
                {
                    if (_redmine.AttachComment(taskId, e.Message.Text))
                    {
                        await _bot.SendTextMessageAsync(e.Message.Chat.Id,
                            "Комментарий прикреплён.");
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(e.Message.Chat.Id,
                            "Не удалось прикрепить комментарий.");
                    }
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id,
                            "Не удалось прикрепить комментарий.");
                }
            }
        }

        private async void SendTasksForUser(MessageEventArgs e)
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
        private async void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            var callbackData = e.CallbackQuery.Data;
            long taskId = long.Parse(callbackData[1..]);
            if(callbackData[0] == 'c')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст комментария.");
                _database.ChangeTelegramChatStatus(taskId, TelegramChatStatus.AuthorizedAndWaitingForComment);
            }
            if (callbackData[0] == 'd')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст статуса.");
                _database.ChangeTelegramChatStatus(taskId, TelegramChatStatus.AuthorizedAndWaitingForChangeStatus);
            }
        }
    }
}
