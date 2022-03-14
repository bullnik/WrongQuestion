using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace RedmineTelegram
{
    sealed public class TelegramBot
    {
        private readonly TelegramBotClient _bot;
        //Token for bot Jijoba @jijoba_bot:
        private string Token => "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";
        private readonly RedmineDatabase _redmineDatabase;

        public TelegramBot(RedmineDatabase redmineDatabase)
        {
            _redmineDatabase = redmineDatabase;
            _bot = new TelegramBotClient(Token);
            _bot.OnMessage += OnMessageHandler;
            _bot.OnCallbackQuery += OnButtonClick;
        }

        public void SendNewIssueToUser()
        {

        }

        public void StartReceiving()
        {
            _bot.StartReceiving();
        }

        public void StopReceiving()
        {
            if (_bot.IsReceiving)
            {
                _bot.StopReceiving();
            }
        }

        private async void GetTasks(long chatId)
        {
            var functions = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Посмотреть мои задачи ☻", "Просмотр задач"),
                }
            });
            await _bot.SendTextMessageAsync(chatId, 
                "Вы успешно авторизованы, выберите функции ниже.", 
                replyMarkup: functions);
        }

        private async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            string text = e.Message.Text;
            if (e.Message.Text == "/start")
            {
                GetTasks(e.Message.Chat.Id);
            }
            else
            {
                await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите команду /start для старта бота.");
            }
        }


        private async void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            var callbackData = e.CallbackQuery.Data;
            var cansel = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Отменить операцию и вернуться к задачам", "Отмена"),
                }
            });
            if (callbackData == "Просмотр задач")
            {
                List<NormalIssue> tasks = _redmineDatabase.GetUserIssues();
                foreach (NormalIssue task in tasks)
                {
                    var editing = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Поменять статус", "s" + task.Id.ToString()),
                            InlineKeyboardButton.WithCallbackData("Поменять трудозатраты", "w" + task.Id.ToString()),
                            InlineKeyboardButton.WithCallbackData("Указать комментарий", "c" + task.Id.ToString()),
                        }
                    });
                    await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Статус задачи: " + task.StatusId + '\n' + "Описание задачи: "
                        + task.Description + '\n' + "Примерное время выполнения: " + task.EstimatedHours, replyMarkup: editing);
                }
            }
            else if (callbackData[0] == 's')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите статус задачи.", replyMarkup: cansel);
                //RedmineDatabase.ChangeTelegramChatStatus(taskId, TelegramChatStatus.AuthorizedAndWaitingForComment);
                //нужно поменять в базе данных статус
            }
            else if (callbackData[0] == 'w')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите трудозатраты.", replyMarkup: cansel);
                //RedmineDatabase.ChangeTelegramChatStatus(taskId, TelegramChatStatus.AuthorizedAndWaitingForComment);
                //нужно поменять в базу данных трудозатраты
            }
            else if (callbackData[0] == 'c')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите комментарий.", replyMarkup: cansel);
                //RedmineDatabase.ChangeTelegramChatStatus(taskId, TelegramChatStatus.AuthorizedAndWaitingForComment);
                //нужно поменять в базу данных комментарий
            }
            else if (callbackData == "Отменить")
            {
                GetTasks(e.CallbackQuery.Message.Chat.Id);
            }
        }
    }
}