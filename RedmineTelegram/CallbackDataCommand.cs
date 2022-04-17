using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public enum CallbackDataCommand
    {
        CancelOperation,
        ShowIssuesList,
        ShowIssueWithoutKeyboardMarkup,
        ShowIssue,
        ShowStatuses,
        AddComment,
        ChangeStatus,
        ChangeLabor
    }
}
