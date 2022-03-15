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
        private static string Token => "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";
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

        public async void SendNewIssueToUser(NormalIssue issue, long telegramUserId)
        {
            var editing = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Поменять статус", "ChangeStatus " + issue.Id.ToString()),
                    InlineKeyboardButton.WithCallbackData("Указать трудозатраты", "ChangeLabor " + issue.Id.ToString())
                }
            });
            await _bot.SendTextMessageAsync(telegramUserId, "На вас назначена задача! " + issue.Status + '\n' + 
                "Описание задачи: " + issue.Description + '\n' + "Ожидаемое время выполнения: " + 
                issue.EstimatedHours, replyMarkup: editing);
        }

        public async void SendClosedIssueToUser(NormalIssue issue, long telegramUserId)
        {
            var editing = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Указать трудозатраты", "ChangeLabor " + issue.Id.ToString())
                }
            });
            await _bot.SendTextMessageAsync(telegramUserId, "Задача закрыта! " + issue.Status + '\n' +
                "Описание задачи: " + issue.Description + '\n' + "Пожалуйста, укажите трудозатраты!" +
                issue.EstimatedHours, replyMarkup: editing);
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
            string userMessage = e.Message.Text;
            long userId = e.Message.Chat.Id;
            bool isUserLoginInRedmine = _redmineDatabase.TryGetRedmineUserIdByTelegram(
                e.Message.From.Username, out _);
            _internalDatabase.InsertUserToDatabase(e.Message.Chat.Id, e.Message.Chat.Username);

            if (!isUserLoginInRedmine)
            {
                await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Укажите ваш Telegram в Redmine для авторизации");
                return;
            }

            var changedIssueAndExpectedAction = _internalDatabase.GetExpectedActionAndChangedIssueByUserId(userId);

            if (changedIssueAndExpectedAction.Item1 == ExpectedAction.Nothing)
            {
                ShowMenu(e.Message.Chat.Id);
            }
            else if (changedIssueAndExpectedAction.Item1 == ExpectedAction.WaitForNewStatusId)
            {
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.Nothing, 0, userId);
            }
            else if (changedIssueAndExpectedAction.Item1 == ExpectedAction.WaitForLaborCosts)
            {
                if (int.TryParse(userMessage, out int laborCost))
                {
                    _redmineDatabase.ChangeLaborCost(changedIssueAndExpectedAction.Item2, laborCost);
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Успешно изменён");
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "Неверный формат времени.");
                }
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.Nothing, 0, userId);
                ShowMenu(userId);
            }
        }

        private async void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            string username = e.CallbackQuery.From.Username;
            if (_internalDatabase.TryGetUserTelegramIdByUsername(username, out long userId))
            {
                _internalDatabase.InsertUserToDatabase(e.CallbackQuery.From.Id, e.CallbackQuery.From.Username);
            }
            bool isUserLoginInRedmine = _redmineDatabase.TryGetRedmineUserIdByTelegram(
                        username, out long redmineUserId);
            string[] callbackData = e.CallbackQuery.Data.Split(' ');
            string command = callbackData[0];
            if (!isUserLoginInRedmine)
            {
                return;
            }
            var cancel = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Отменить операцию и вернуться к задачам", "Cancel"),
                }
            });
            if (command == "WatchIssues")
            {
                List<NormalIssue> tasks = _redmineDatabase.GetUserIssues(redmineUserId);
                WatchIssues(e.CallbackQuery.Message.Chat.Id, tasks);
            }
            else if (command == "ChangeStatus")           
            {
                string status = "";
                if (callbackData.Length >= 2)
                { 
                    for (int i=1; i < callbackData.Length-1; i++)
                    {
                        status += callbackData[i] + " ";
                    }
                    status = status.Substring(0, status.Length-1);
                }
                int statusId = _redmineDatabase.GetStatusIdByName(status);
                long issueId = long.Parse(callbackData[callbackData.Length-1]);
                if (_redmineDatabase.ChangeIssueStatus(issueId, statusId))
                {
                    await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, 
                        "Статус задачи изменен.");
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, 
                        "Произошла ошибка при изменении статуса.");
                }
            }
            else if (command == "ViewStatus")
            {
                var listButtons = new List<InlineKeyboardButton>();
                foreach (var status in _redmineDatabase.GetStatusesList())
                {
                    listButtons.Add(InlineKeyboardButton.WithCallbackData(status, "ChangeStatus " + status + " " + callbackData[1]));
                }

                var buttontsStatus = new InlineKeyboardMarkup(new[]
                {
                    listButtons.ToArray(),
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Отменить операцию и вернуться к задачам", "Cancel")
                    }
                });

                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите статус задачи.", replyMarkup: buttontsStatus);
                long issueId = long.Parse(callbackData[1]);
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.WaitForNewStatusId, issueId, userId);
            }
            else if (command == "ChangeLabor")
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите трудозатраты (в часах).", replyMarkup: cancel);
                long issueId = long.Parse(callbackData[1]);
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.WaitForLaborCosts, issueId, userId);
            }
            else if (command == "Cancel")
            {
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.Nothing, 0, userId);
                List<NormalIssue> tasks = _redmineDatabase.GetUserIssues(redmineUserId);
                WatchIssues(e.CallbackQuery.Message.Chat.Id, tasks);
            }
        }

        private async void SendIssue(long chatId, NormalIssue issue)
        {
            var editing = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Поменять статус", "ViewStatus " + issue.Id.ToString()),
                    InlineKeyboardButton.WithCallbackData("Указать трудозатраты", "ChangeLabor " + issue.Id.ToString())
                }
            });
            await _bot.SendTextMessageAsync(chatId, "Статус задачи: " + issue.Status + '\n' + "Описание задачи: "
                + issue.Description + '\n' + "Примерное время выполнения: " + issue.EstimatedHours, replyMarkup: editing);
        }

        private void WatchIssues(long chatId, List<NormalIssue> tasks)
        {
            foreach (NormalIssue task in tasks)
            {
                SendIssue(chatId, task);
            }
        }

        private async void ShowMenu(long chatId)
        {
            var functions = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Посмотреть мои задачи ☻", "WatchIssues"),
                }
            });
            await _bot.SendTextMessageAsync(chatId,
                "Вы успешно авторизованы в Redmine.",
                replyMarkup: functions);
        }
    }
}