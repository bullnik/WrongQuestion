using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace RedmineTelegram
{
    public sealed class ReplyMarkups
    {
        public static InlineKeyboardMarkup GetIssuesSubjectWithShowIssueCallbackData(List<Issue> issues)
        {
            List<InlineKeyboardButton[]> keyboardButtons = new();

            foreach (Issue issue in issues)
            {
                keyboardButtons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(issue.Subject,
                    new CallbackData(CallbackDataCommand.ShowIssue, issue.Id).ToString())
                }); ;
            }

            return new(keyboardButtons.ToArray());
        }

        public static InlineKeyboardMarkup GetShowInfoWithShowIssueWithoutKeyboardMarkupCallbackData(long issueId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Информация о задаче",
                    new CallbackData(CallbackDataCommand.ShowIssueWithoutKeyboardMarkup, issueId).ToString())
                }
            });
        }

        public static InlineKeyboardMarkup GetShowInfoWithShowIssueCallbackData(long issueId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Информация о задаче",
                    new CallbackData(CallbackDataCommand.ShowIssue, issueId).ToString())
                }
            });
        }

        public static InlineKeyboardMarkup GetIssueEdit(long issueId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Поменять статус",
                        new CallbackData(CallbackDataCommand.ShowStatuses, issueId).ToString())
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Указать трудозатраты",
                        new CallbackData(CallbackDataCommand.ChangeLabor, issueId).ToString())
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Добавить комментарий", 
                        new CallbackData(CallbackDataCommand.AddComment, issueId).ToString())
                }
            });
        }

        public static readonly InlineKeyboardMarkup CancelOperation = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Отменить операцию", 
                    new CallbackData(CallbackDataCommand.CancelOperation).ToString())
            }
        });

        public static readonly InlineKeyboardMarkup ListIssues = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Посмотреть мои задачи", 
                    new CallbackData(CallbackDataCommand.ShowIssuesList).ToString())
            }
        });

        public static InlineKeyboardMarkup GetStatusButtons(long issueId)
        {
            List<InlineKeyboardButton[]> lines = new();

            List<string> line = new();
            foreach (string status in RedmineAccessController.GetStatusesList())
            {
                line.Add(status);

                if (line.Count == 2)
                {
                    lines.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData(line[0],
                            new CallbackData(CallbackDataCommand.ChangeStatus, issueId, line[0]).ToString()),
                        InlineKeyboardButton.WithCallbackData(line[1],
                            new CallbackData(CallbackDataCommand.ChangeStatus, issueId, line[1]).ToString())
                    });
                    line = new();
                }
            }

            if (line.Count == 1)
            {
                lines.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(line[0],
                        new CallbackData(CallbackDataCommand.ChangeStatus, issueId, line[0]).ToString())
                });
            }

            lines.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("Отменить операцию",
                    new CallbackData(CallbackDataCommand.CancelOperation).ToString())
            });

            return new InlineKeyboardMarkup(lines.ToArray());
        }
    }
}
