using System;
using System.Collections.Generic;
using System.Threading;

namespace WrongQuestion
{
    class Program
    {
        static void Main()
        {
            TelegramBot bot = new TelegramBot();
            bot.Run();

            while(true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
