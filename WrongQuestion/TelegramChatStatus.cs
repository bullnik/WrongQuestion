using System;
using System.Collections.Generic;
using System.Text;

namespace WrongQuestion
{
    public enum TelegramChatStatus
    {
        Unauthorized = 0,
        Authorized = 1,
        AuthorizedAndWaitingForChangeStatus = 2,
        AuthorizedAndWaitingForComment = 3
    }
}
