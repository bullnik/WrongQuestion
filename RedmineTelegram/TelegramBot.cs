using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RedmineTelegram
{
    public sealed class TelegramBot
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
                await _bot.SendTextMessageAsync(telegramUserId, 
                    "Укажите ваш Telegram в Redmine для авторизации");
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
                if (float.TryParse(parts[0], out float laborCost))
                {
                    if (RedmineAccessController.AddLaborCost(changedIssueId, laborCost, comment, redmineUserId))
                    {
                        await _bot.SendTextMessageAsync(telegramUserId, "✅ <b>Трудозатраты добавлены</b>",
                            replyMarkup: ReplyMarkups.ListIssues,
                            parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(telegramUserId, "❌ <b>Произошла ошибка</b>",
                            replyMarkup: ReplyMarkups.ListIssues,
                            parseMode: ParseMode.Html);
                    }
                    _redmineAccessController.ResetExpectedActionAndIssueByTelegramUserId(telegramUserId);
                }
                else
                {
                    await _bot.SendTextMessageAsync(telegramUserId, 
                        "❌ <b>Неверный формат времени</b>",
                        replyMarkup: ReplyMarkups.CancelOperation, 
                        parseMode: ParseMode.Html);
                }
            }
            else if (expectedAction == ExpectedAction.WaitForComment)
            {
                if (RedmineAccessController.AddComment(changedIssueId, userMessage, redmineUserId))
                {
                    await _bot.SendTextMessageAsync(telegramUserId, 
                        "✅ <b>Комментарий добавлен</b>",
                        replyMarkup: ReplyMarkups.ListIssues,
                        parseMode: ParseMode.Html);
                }
                else
                {
                    await _bot.SendTextMessageAsync(telegramUserId, 
                        "❌ <b>Произошла ошибка</b>",
                        replyMarkup: ReplyMarkups.ListIssues,
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

            CallbackData callbackData = CallbackData.GetFromString(e.CallbackQuery.Data);

            SwitchCallbackData(telegramUserId, redmineUserId, callbackData);
        }

        private async void SwitchCallbackData(long telegramUserId, long redmineUserId, CallbackData callbackData)
        {
            CallbackDataCommand command = callbackData.Command;

            if (command == CallbackDataCommand.ShowIssuesList)
            {
                List<Issue> tasks = RedmineAccessController.GetUserIssuesByRedmineUserId(redmineUserId);
                ShowIssuesList(telegramUserId, tasks);
            }
            else if (command == CallbackDataCommand.ShowIssueWithoutKeyboardMarkup)
            {
                Issue issue = RedmineAccessController.GetIssueByIssueId(callbackData.TargetIssueId);
                SendIssueWithoutEditingMarkup(telegramUserId, issue);
            }
            else if (command == CallbackDataCommand.ShowIssue)
            {
                Issue issue = RedmineAccessController.GetIssueByIssueId(callbackData.TargetIssueId);
                SendIssueWithEditingMarkup(telegramUserId, issue);
            }
            else if (command == CallbackDataCommand.AddComment)
            {
                _redmineAccessController.ChangeExpectedActionAndIssueByTelegramUserId(
                    ExpectedAction.WaitForComment, callbackData.TargetIssueId, telegramUserId);
                await _bot.SendTextMessageAsync(telegramUserId, "📝 Введите комментарий",
                    replyMarkup: ReplyMarkups.CancelOperation, parseMode: ParseMode.Html);
            }
            else if (command == CallbackDataCommand.ChangeStatus)
            {
                string status = callbackData.AdditionalData;

                if (RedmineAccessController.ChangeStatus(callbackData.TargetIssueId, status, redmineUserId))
                {
                    await _bot.SendTextMessageAsync(telegramUserId,
                        "✅ <b>Статус задачи изменен</b>", 
                        replyMarkup: ReplyMarkups.ListIssues,
                        parseMode: ParseMode.Html);
                }
                else
                {
                    await _bot.SendTextMessageAsync(telegramUserId,
                        "❌ <b>Произошла ошибка при изменении статуса</b>",
                        replyMarkup: ReplyMarkups.ListIssues, 
                        parseMode: ParseMode.Html);
                }
            }
            else if (command == CallbackDataCommand.ShowStatuses)
            {
                _redmineAccessController.ChangeExpectedActionAndIssueByTelegramUserId(
                    ExpectedAction.WaitForNewStatusId, callbackData.TargetIssueId, telegramUserId);
                await _bot.SendTextMessageAsync(telegramUserId, 
                    "📝 Выберите новый статус задачи",
                    replyMarkup: ReplyMarkups.GetStatusButtons(callbackData.TargetIssueId), 
                    parseMode: ParseMode.Html);
            }
            else if (command == CallbackDataCommand.ChangeLabor)
            {
                _redmineAccessController.ChangeExpectedActionAndIssueByTelegramUserId(
                    ExpectedAction.WaitForLaborCosts, callbackData.TargetIssueId, telegramUserId);
                await _bot.SendTextMessageAsync(telegramUserId, 
                    "📝 Введите трудозатраты (в часах)" + '\n' 
                    + "И на что они потрачены, через пробел" + '\n' 
                    + "Пример: 4,5 работал",
                    replyMarkup: ReplyMarkups.CancelOperation, 
                    parseMode: ParseMode.Html);
            }
            else if (command == CallbackDataCommand.CancelOperation)
            {
                _redmineAccessController.ResetExpectedActionAndIssueByTelegramUserId(telegramUserId);
                ShowMenu(telegramUserId);
            }
        }

        private async void SendIssueWithEditingMarkup(long telegramUserId, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, 
                GetIssueInfo(issue), 
                replyMarkup: ReplyMarkups.GetIssueEdit(issue.Id),
                parseMode: ParseMode.Html);
        }

        private async void SendIssueWithoutEditingMarkup(long telegramUserId, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, 
                GetIssueInfo(issue),
                parseMode: ParseMode.Html);
        }

        private static string GetIssueInfo(Issue issue)
        {
            return $"{issue.Link}: {issue.Subject}" + '\n'
                + $"Статус: {issue.Status}" + '\n'
                + $"Описание: {issue.Description}" + '\n'
                + $"Приоритет: {issue.Priority}" + '\n'
                + $"Трудозатраты: {Math.Round(issue.LaborCostsSum, 2)} ч." + '\n'
                + $"Назначена с {issue.CreatedOn}" + '\n';
        }

        private async void ShowIssuesList(long chatId, List<Issue> issues)
        {
            if (issues.Count == 0)
            {
                await _bot.SendTextMessageAsync(chatId, 
                    "⚡️ <b>Задач нет</b>", 
                    parseMode: ParseMode.Html);
                return;
            }

            await _bot.SendTextMessageAsync(chatId, 
                "⚡️ <b>Ваши задачи: </b>",
                replyMarkup: ReplyMarkups.GetIssuesSubjectWithShowIssueCallbackData(issues), 
                parseMode: ParseMode.Html);
        }

        private async void ShowMenu(long chatId)
        {
            await _bot.SendTextMessageAsync(chatId, 
                "Вы успешно авторизованы в Redmine.",
                replyMarkup: ReplyMarkups.ListIssues);
        }

        internal async void SendStatusChangeNotificationToWatcherOrCreator(long telegramUserId, 
            JournalItem journal, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId,
                $"⚡️ {journal.UserName} изменил статус задачи {issue.Link}: {issue.Subject} "
                + $"с \"{journal.OldIssueStatus}\" на \"{journal.CurrentIssueStatus}\"",
                replyMarkup: ReplyMarkups.GetShowInfoWithShowIssueWithoutKeyboardMarkupCallbackData(issue.Id),
                parseMode: ParseMode.Html);
        }

        internal async void SendStatusChangeNotificationToAssignedUser(long telegramUserId, 
            JournalItem journal, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId, 
                $"⚡️ {journal.UserName} изменил статус задачи {issue.Link}: {issue.Subject} "
                + $"с \"{journal.OldIssueStatus}\" на \"{journal.CurrentIssueStatus}\"",
                replyMarkup: ReplyMarkups.GetShowInfoWithShowIssueCallbackData(issue.Id),
                parseMode: ParseMode.Html);
        }

        internal async void SendCommentNotificationToWatcherOrCreator(long telegramUserId, 
            JournalItem journal, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId,
                $"⚡️ {journal.UserName} добавил комментарий к задаче {issue.Link}: {issue.Subject}:" + '\n'
                + journal.Comment,
                replyMarkup: ReplyMarkups.GetShowInfoWithShowIssueWithoutKeyboardMarkupCallbackData(issue.Id),
                parseMode: ParseMode.Html);
        }

        internal async void SendCommentNotificationToAssignedUser(long telegramUserId, 
            JournalItem journal, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId,
                $"⚡️ {journal.UserName} добавил комментарий к задаче {issue.Link}: {issue.Subject}:" + '\n' 
                + journal.Comment,
                replyMarkup: ReplyMarkups.GetShowInfoWithShowIssueCallbackData(issue.Id),
                parseMode: ParseMode.Html);
        }

        internal async void SendNewIssueToWatcher(long telegramUserId, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId,
                $"⚡️ {issue.CreatorName} назначил вас наблюдалетем за задачей {issue.Link}: {issue.Subject}",
                replyMarkup: ReplyMarkups.GetShowInfoWithShowIssueWithoutKeyboardMarkupCallbackData(issue.Id),
                parseMode: ParseMode.Html);
        }

        internal async void SendNewIssueToAssignedUser(long telegramUserId, Issue issue)
        {
            await _bot.SendTextMessageAsync(telegramUserId,
                $"⚡️ {issue.CreatorName} назначил на вас новую задачу {issue.Link}: {issue.Subject}",
                replyMarkup: ReplyMarkups.GetShowInfoWithShowIssueCallbackData(issue.Id),
                parseMode: ParseMode.Html);
        }
    }
}