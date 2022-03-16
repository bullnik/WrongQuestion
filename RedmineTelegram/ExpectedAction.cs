using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public enum ExpectedAction
    {
        Nothing = 0,
        WaitForNewStatusId = 1,
        WaitForLaborCosts = 2,
        WaitForComment = 3
    }
}
