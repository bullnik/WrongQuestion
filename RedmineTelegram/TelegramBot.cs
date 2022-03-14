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
        private readonly InternalDatabase _internalDatabase;

        public TelegramBot(RedmineDatabase redmineDatabase, InternalDatabase internalDatabase)
        {
            _redmineDatabase = redmineDatabase;
            _internalDatabase = internalDatabase;
            _bot = new TelegramBotClient(Token);
            _bot.OnMessage += OnMessageHandler;
            _bot.OnCallbackQuery += OnButtonClick;
        }

        public async void SendNewIssueToUser(long telegramUserId)
        {
            await _bot.SendTextMessageAsync(telegramUserId, "hello");
        }

        public void SendClosedIssueToUser(long telegramUserId)
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

        private async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            string text = e.Message.Text;
            long userId = e.Message.Chat.Id;
            bool isUserLoginInRedmine = _redmineDatabase.TryGetRedmineUserIdByTelegram(
                e.Message.From.Username, out long redmineUserId);
            _internalDatabase.InsertUserToDatabase(e.Message.Chat.Id, e.Message.Chat.Username);

            if (!isUserLoginInRedmine)
            {
                await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Укажите ваш Telegram в Redmine для авторизации");
                return;
            }

            var changedIssueAndExpectedAction = _internalDatabase.GetChangedIssueAndExpectedActionByUserId(userId);

            if (changedIssueAndExpectedAction.Item1 == ExpectedAction.Nothing)
            {
                ShowMenu(e.Message.Chat.Id);
            }
            else if (changedIssueAndExpectedAction.Item1 == ExpectedAction.WaitForNewStatusId)
            {

            }
            else if (changedIssueAndExpectedAction.Item1 == ExpectedAction.WaitForLaborCosts)
            {

            }
        }

        private async void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            var callbackData = e.CallbackQuery.Data;
            string username = e.CallbackQuery.From.Username;
            bool userSavedInDatabase = _internalDatabase.TryGetUserTelegramIdByUsername(username, out long userId);
            bool isUserLoginInRedmine = _redmineDatabase.TryGetRedmineUserIdByTelegram(
                        username, out long redmineUserId);
            if (!isUserLoginInRedmine)
            {
                return;
            }
            var cancel = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Отменить операцию и вернуться к задачам", "Отмена"),
                }
            });
            if (callbackData == "Просмотр задач")
            {
                List<NormalIssue> tasks = _redmineDatabase.GetUserIssues(redmineUserId);
                WatchIssues(e.CallbackQuery.Message.Chat.Id, tasks);
            }
            else if (callbackData[0] == 's')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите статус задачи.", replyMarkup: cancel);
                long issueId = long.Parse(callbackData[1..]);
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.WaitForNewStatusId, issueId);
            }
            else if (callbackData[0] == 'w')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите трудозатраты.", replyMarkup: cancel);
                long issueId = long.Parse(callbackData[1..]);
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.WaitForLaborCosts, issueId);
            }
            else if (callbackData == "Отмена")
            {
                List<NormalIssue> tasks = _redmineDatabase.GetUserIssues(redmineUserId);
                WatchIssues(e.CallbackQuery.Message.Chat.Id, tasks);
            }
        }

        private async void WatchIssues(long chatId, List<NormalIssue> tasks)
        {
            foreach (NormalIssue task in tasks)
            {
                var editing = new InlineKeyboardMarkup(new[]
                {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Поменять статус", "s" + task.Id.ToString()),
                            InlineKeyboardButton.WithCallbackData("Поменять трудозатраты", "w" + task.Id.ToString())
                        }
                    });
                await _bot.SendTextMessageAsync(chatId, "Статус задачи: " + task.Status + '\n' + "Описание задачи: "
                    + task.Description + '\n' + "Примерное время выполнения: " + task.EstimatedHours, replyMarkup: editing);
            }
        }

        private async void ShowMenu(long chatId)
        {
            var functions = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Посмотреть мои задачи ☻", "Просмотр задач"),
                }
            });
            await _bot.SendTextMessageAsync(chatId,
                "Вы успешно авторизованы, выберите функции ниже.",
                replyMarkup: functions);
        }
    }
}