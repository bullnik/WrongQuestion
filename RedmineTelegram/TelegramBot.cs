using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

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
                    InlineKeyboardButton.WithCallbackData("Поменять статус", "ViewStatus " + issue.Id.ToString()),
                    InlineKeyboardButton.WithCallbackData("Указать трудозатраты", "ChangeLabor " + issue.Id.ToString())
                }
            });
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️На вас назначена задача</b>⚡️" + '\n' + "Статус: " + issue.Status + '\n' + "Название: " + issue.Subject + '\n' + "Описание: "
                + issue.Description + '\n' + "Приоритет: " + issue.Priority + '\n' + "Примерное время выполнения: " + issue.EstimatedHours + " ч."
                + '\n' + "Назначена с " + issue.CreatedOn, replyMarkup: editing, parseMode: ParseMode.Html);
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
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️Задача закрыта</b>⚡️" + '\n' + "Название: " + issue.Subject + '\n' + "Описание: "
                + issue.Description + '\n' + "Пожалуйста, укажите трудозатраты (если не указаны раньше)", replyMarkup: editing, parseMode: ParseMode.Html);
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
                string[] parts = userMessage.Split(' ');
                string comment = userMessage[parts[0].Length..];
                if (int.TryParse(parts[0], out int laborCost))
                {
                    _redmineDatabase.ChangeLaborCost(changedIssueAndExpectedAction.Item2, laborCost, comment, 
                        e.Message.From.Username);
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "<b>✅Успешно изменён</b>✅", 
                        parseMode: ParseMode.Html);
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.Message.Chat.Id, "<b>❌Неверный формат времени</b>❌", 
                        parseMode: ParseMode.Html);
                }
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.Nothing, 0, userId);
                ShowMenu(userId);
            }
            else if (changedIssueAndExpectedAction.Item1 == ExpectedAction.WaitForComment)
            {
                _redmineDatabase.AddComment(changedIssueAndExpectedAction.Item2, userMessage, e.Message.From.Username);
                await _bot.SendTextMessageAsync(e.Message.Chat.Id, "✅<b>Комментарий добавлен</b>✅", 
                    parseMode: ParseMode.Html);
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.Nothing, 0, userId);
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
            else if (command == "AddComment")
            {
                var buttontsStatus = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Отменить операцию и вернуться к задачам", "Cancel")
                    }
                });

                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, 
                    "📝Введите комментарий📝", replyMarkup: buttontsStatus, parseMode: ParseMode.Html);
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.WaitForComment, 
                    long.Parse(callbackData[1]), userId);
            }
            else if (command == "ChangeStatus")           
            {
                if (callbackData.Length < 3)
                {
                    return;
                }
                string status = "";
                if (callbackData.Length >= 2)
                { 
                    for (int i=1; i < callbackData.Length-1; i++)
                    {
                        status += callbackData[i] + " ";
                    }
                    status = status[0..^1];
                }
                int statusId = _redmineDatabase.GetStatusIdByName(status);
                long issueId = long.Parse(callbackData[^1]);
                if (_redmineDatabase.ChangeIssueStatus(issueId, statusId))
                {
                    await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id,
                        "<b>✅Статус задачи изменен✅</b>", parseMode: ParseMode.Html);
                }
                else
                {
                    await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id,
                        "<b>❌Произошла ошибка при изменении статуса❌</b>", parseMode: ParseMode.Html);
                }
            }
            else if (command == "ViewStatus")
            {
                var listButtons = new List<InlineKeyboardButton>();
                foreach (var status in _redmineDatabase.GetStatusesList())
                {
                    listButtons.Add(InlineKeyboardButton.WithCallbackData(status, "ChangeStatus " 
                        + status + " " + callbackData[1]));
                }

                var buttontsStatus = new InlineKeyboardMarkup(new[]
                {
                    listButtons.ToArray(),
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Отменить операцию и вернуться к задачам", "Cancel")
                    }
                });

                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "📝Введите статус задачи📝", 
                    replyMarkup: buttontsStatus, parseMode: ParseMode.Html);
                long issueId = long.Parse(callbackData[1]);
                _internalDatabase.ChangeIssueAndExpectedActionByUserId(ExpectedAction.WaitForNewStatusId, issueId, userId);
            }
            else if (command == "ChangeLabor")
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "📝Введите трудозатраты (в часах)📝" 
                    + '\n' + "И на что они потрачены, через пробел", replyMarkup: cancel, parseMode: ParseMode.Html);
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
                    InlineKeyboardButton.WithCallbackData("Указать трудозатраты", "ChangeLabor " + issue.Id.ToString()),
                    InlineKeyboardButton.WithCallbackData("Добавить комментарий", "AddComment " + issue.Id.ToString())
                }
            });
            await _bot.SendTextMessageAsync(chatId, "<b>⚡️Информация о задаче⚡️</b>" + '\n' + "Статус: " + issue.Status + '\n' + "Название: " + issue.Subject + '\n' + "Описание: "
                + issue.Description + '\n' + "Приоритет: " + issue.Priority + '\n' + "Примерное время выполнения: " + issue.EstimatedHours + " ч."
                + '\n' + "Назначена с " + issue.CreatedOn, replyMarkup: editing, parseMode: ParseMode.Html);

        }

        private async void WatchIssues(long chatId, List<NormalIssue> tasks)
        {
            foreach (NormalIssue task in tasks)
            {
                SendIssue(chatId, task);
            }
            if (tasks.Count == 0)
                await _bot.SendTextMessageAsync(chatId, "<b>⚡️Задач нет⚡️</b>", parseMode: ParseMode.Html);
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