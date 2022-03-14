using System;
using System.Collections.Generic;

namespace WrongQuestion
{
    class Program
    {
        static void Main()
        {
            TelegramBot bot = new TelegramBot();
            bot.Run();
            Console.ReadKey();
        }
    }
}
