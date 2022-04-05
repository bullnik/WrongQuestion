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
        private readonly RedmineAccessController _redmineAccessController;

        public TelegramBot(RedmineAccessController redmineAccessController)
        {
            _redmineAccessController = redmineAccessController;
            _bot = new TelegramBotClient(Token);
            _bot.OnMessage += OnMessageHandler;
            _bot.OnCallbackQuery += OnButtonClick;
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
            long telegramUserId = e.Message.Chat.Id;
            string telegramUsername = e.Message.Chat.Username;

            if (userMessage is null)
            {
                return;
            }

            if (!_redmineAccessController.VerifyRedmineUserByTelegramIdAndUsername(
                telegramUserId, telegramUsername, out long redmineUserId))
            {
                await _bot.SendTextMessageAsync(telegramUserId, "Укажите ваш Telegram в Redmine для авторизации");
                return;
            }

            if (userMessage.Length > 400)
            {
                userMessage = e.Message.Text[..400];
            }

            SwitchMessage(telegramUserId, userMessage, redmineUserId);
        }

        private async void SwitchMessage(long telegramUserId, string userMessage, long redmineUserId)
        {
            var changedIssueAndExpectedAction = _redmineAccessController
                .GetExpectedActionAndChangedIssueByUserId(telegramUserId);
            long changedIssueId = changedIssueAndExpectedAction.Item2;
            ExpectedAction expectedAction = changedIssueAndExpectedAction.Item1;

            if (expectedAction == ExpectedAction.Nothing)
            {
                ShowMenu(telegramUserId);
            }
            else if (expectedAction == ExpectedAction.WaitForNewStatusId)
            {
                _redmineAccessController.ResetExpectedActionAndIssueByTelegramUserId(telegramUserId);
            }
            else if (expectedAction == ExpectedAction.WaitForLaborCosts)
            {
                string[] parts = userMessage.Split(' ');
                string comment = userMessage[parts[0].Length..];
                if (double.TryParse(parts[0], out double laborCost))
                {
                    if (_redmineAccessController.AddLaborCost(changedIssueId, laborCost, comment, redmineUserId))
                    {
                        await _bot.SendTextMessageAsync(telegramUserId, "✅ <b>Трудозатраты добавлены</b>",
                            parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(telegramUserId, "❌ <b>Произошла ошибка</b>",
                            parseMode: ParseMode.Html);
                    }
                    _redmineAccessController.ResetExpectedActionAndIssueByTelegramUserId(telegramUserId);
                }
                else
                {
                    await _bot.SendTextMessageAsync(telegramUserId, "❌ <b>Неверный формат времени</b>",
                        replyMarkup: CancelKeyboardMarkup, parseMode: ParseMode.Html);
                }
            }
            else if (expectedAction == ExpectedAction.WaitForComment)
            {
                if (_redmineAccessController.AddComment(changedIssueId, userMessage, redmineUserId))
                {
                    await _bot.SendTextMessageAsync(telegramUserId, "✅ <b>Комментарий добавлен</b>",
                                    parseMode: ParseMode.Html);
                }
                else
                {
                    await _bot.SendTextMessageAsync(telegramUserId, "❌ <b>Произошла ошибка</b>",
                            parseMode: ParseMode.Html);
                }
                _redmineAccessController.ResetExpectedActionAndIssueByTelegramUserId(telegramUserId);
            }
        }

        private void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            string telegramUsername = e.CallbackQuery.From.Username;
            long telegramUserId = e.CallbackQuery.From.Id;

            if (!_redmineAccessController.VerifyRedmineUserByTelegramIdAndUsername(
                telegramUserId, telegramUsername, out long redmineUserId))
            {
                return;
            }

            string[] callbackData = e.CallbackQuery.Data.Split(' ');
            string command = callbackData[0];

            SwitchButton(telegramUsername, telegramUserId, callbackData, command, redmineUserId);
        }

        private async void SwitchButton(string telegramUsername, long telegramUserId, 
            string[] callbackData, string command, long redmineUserId)
        {
            if (command == "WatchIssues")
            {
                List<NormalIssue> tasks = _redmineAccessController.GetUserIssuesByRedmineUserId(redmineUserId);
                ShowIssues(telegramUserId, tasks);
            }
            else if (command == "WatchIssue")
            {
                long issueId = long.Parse(callbackData[1]);
                NormalIssue issue = _redmineAccessController.GetIssueByIssueId(issueId);
                SendIssue(telegramUserId, issue);
            }
            else if (command == "AddComment")
            {
                await _bot.SendTextMessageAsync(telegramUserId, "📝 Введите комментарий", 
                    replyMarkup: CancelKeyboardMarkup, parseMode: ParseMode.Html);
                _redmineAccessController.ChangeExpectedActionAndIssueByTelegramUserId(
                    ExpectedAction.WaitForComment, long.Parse(callbackData[1]), telegramUserId);
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
                    for (int i = 1; i < callbackData.Length - 1; i++)
                    {
                        status += callbackData[i] + " ";
                    }
                    status = status[0..^1];
                }
                long issueId = long.Parse(callbackData[^1]);

                if (_redmineAccessController.ChangeStatus(issueId, status, redmineUserId))
                {
                    await _bot.SendTextMessageAsync(telegramUserId,
                        "✅ <b>Статус задачи изменен</b>", parseMode: ParseMode.Html);
                }
                else
                {
                    await _bot.SendTextMessageAsync(telegramUserId,
                        "❌ <b>Произошла ошибка при изменении статуса</b>", parseMode: ParseMode.Html);
                }
            }
            else if (command == "ViewStatus")
            {
                long issueId = long.Parse(callbackData[1]);
                await _bot.SendTextMessageAsync(telegramUserId, "📝 Выберите новый статус задачи",
                    replyMarkup: GetStatusButtons(issueId), parseMode: ParseMode.Html);
                _redmineAccessController.ChangeExpectedActionAndIssueByTelegramUserId(
                    ExpectedAction.WaitForNewStatusId, issueId, telegramUserId);
            }
            else if (command == "ChangeLabor")
            {
                await _bot.SendTextMessageAsync(telegramUserId, "📝 Введите трудозатраты (в часах)"
                    + '\n' + "И на что они потрачены, через пробел" + '\n' + "Пример: 4,5 работал",
                    replyMarkup: CancelKeyboardMarkup, parseMode: ParseMode.Html);
                long issueId = long.Parse(callbackData[1]);
                _redmineAccessController.ChangeExpectedActionAndIssueByTelegramUserId(
                    ExpectedAction.WaitForLaborCosts, issueId, telegramUserId);
            }
            else if (command == "Cancel")
            {
                _redmineAccessController.ResetExpectedActionAndIssueByTelegramUserId(telegramUserId);
                ShowMenu(telegramUserId);
            }
        }

        private async void SendIssue(long chatId, NormalIssue issue)
        {
            await _bot.SendTextMessageAsync(chatId, "<b>⚡️Информация о задаче⚡️</b>" + '\n' 
                + "Статус: " + issue.Status + '\n' 
                + "Название: " + issue.Subject + '\n' 
                + "Описание: " + issue.Description + '\n' 
                + "Приоритет: " + issue.Priority + '\n' 
                + "Трудозатраты: " + issue.EstimatedHours + " ч."+ '\n' 
                + "Назначена с " + issue.CreatedOn, 
                replyMarkup: GetIssueEditingMarkup(issue.Id), parseMode: ParseMode.Html);
        }

        private async void ShowIssues(long chatId, List<NormalIssue> issues)
        {
            if (issues.Count == 0)
            {
                await _bot.SendTextMessageAsync(chatId, "⚡️ <b>Задач нет</b>", parseMode: ParseMode.Html);
                return;
            }

            await _bot.SendTextMessageAsync(chatId, "⚡️ <b>Ваши задачи: </b>",
                replyMarkup: GetIssuesKeyboardMarkup(issues), parseMode: ParseMode.Html);
        }

        private async void ShowMenu(long chatId)
        {
            await _bot.SendTextMessageAsync(chatId, "Вы успешно авторизованы в Redmine.",
                replyMarkup: ShowIssuesKeyboardMarkup);
        }

        private static InlineKeyboardMarkup GetIssuesKeyboardMarkup(List<NormalIssue> issues)
        {
            List<InlineKeyboardButton[]> keyboardButtons = new();

            foreach (NormalIssue issue in issues)
            {
                keyboardButtons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(issue.Subject,
                    "WatchIssue " + issue.Id.ToString())
                });
            }

            return new(keyboardButtons.ToArray());
        }

        private static InlineKeyboardMarkup GetIssueEditingMarkup(long issueId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Поменять статус", "ViewStatus " + issueId.ToString()),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Указать трудозатраты", "ChangeLabor " + issueId.ToString()),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Добавить комментарий", "AddComment " + issueId.ToString())
                }
            });
        }

        private static readonly InlineKeyboardMarkup CancelKeyboardMarkup = new(new[] 
        { 
            CancelKeyboardButton
        });

        private static readonly InlineKeyboardButton[] CancelKeyboardButton = new[]
        {
            InlineKeyboardButton.WithCallbackData("Отменить операцию", "Cancel")
        };

        private static readonly InlineKeyboardMarkup ShowIssuesKeyboardMarkup = new(new[] 
        {
            ShowIssuesKeyboardButton
        });

        private static readonly InlineKeyboardButton[] ShowIssuesKeyboardButton = new[]
        {
            InlineKeyboardButton.WithCallbackData("Посмотреть мои задачи", "WatchIssues")
        };

        private InlineKeyboardMarkup GetStatusButtons(long issueId)
        { 
            var firstListButtuns = new List<InlineKeyboardButton>();
            var secondRowButtuns = new List<InlineKeyboardButton>();
            var chet = 1;
            foreach (var status in _redmineAccessController.GetStatusesList())
            {
                if (chet % 2 == 0)
                {
                    firstListButtuns.Add(InlineKeyboardButton.WithCallbackData(status, "ChangeStatus "
                        + status + " " + issueId));
                    chet++;
                }
                else
                {
                    secondRowButtuns.Add(InlineKeyboardButton.WithCallbackData(status, "ChangeStatus "
                        + status + " " + issueId));
                    chet++;
                }
            }

            return new InlineKeyboardMarkup(
                new[]
                {
                    firstListButtuns.ToArray(),
                    secondRowButtuns.ToArray(),
                    CancelKeyboardButton
                });
        }

        internal async void SendStatusChangeNotificationToWatcherOrCreator(long telegramUserId, 
            JournalItem journal, NormalIssue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️Стутус задачи \"" + issue.Subject + "\"" + " изменился на: \""
                + issue.Status + "\"⚡️" + "\n" + "Ссылка на задачу: " + issue.Link);
        }

        internal async void SendStatusChangeNotificationToAssignedUser(long telegramUserId, 
            JournalItem journal, NormalIssue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️Стутус задачи \"" + issue.Subject + "\"" + " изменился на: \""
                + issue.Status + "\"⚡️" + "\n" + "Ссылка на задачу: " + issue.Link);
        }

        internal async void SendCommentNotificationToWatcherOrCreator(long telegramUserId, 
            JournalItem journal, NormalIssue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️Добавлен новый комментарий к задаче \"" + issue.Subject + "\"⚡️"
                + "\n" + journal.Comment + "\n" + "Ссылка на задачу: " + issue.Link);
        }

        internal async void SendCommentNotificationToAssignedUser(long telegramUserId, 
            JournalItem journal, NormalIssue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️Добавлен новый комментарий к задаче \"" + issue.Subject + "\"⚡️"
                + "\n" + journal.Comment + "\n" + "Ссылка на задачу: " + issue.Link);
        }

        internal async void SendNewIssueToWatcherOrCreator(long telegramUserId, NormalIssue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️Вы назначены смотрящим за задачей  \"" + issue.Subject + "\"⚡️"
                + "\n" + "Ссылка на задачу: " + issue.Link);
        }

        internal async void SendNewIssueToAssignedUser(long telegramUserId, NormalIssue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, "<b>⚡️На вас назначена задача \"" + issue.Subject + "\"⚡️"
                + "\n" + "Ссылка на задачу: " + issue.Link);
        }
    }
}