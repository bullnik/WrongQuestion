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
        }

        static void Test()
        {
            Database db = new Database();
            db.CreateTableComments();
            db.CreateTableTasks();
            db.InsertTask
            (
                new RedmineTask
                (
                    76457,
                    Tracker.Defect,
                    "Jopa",
                    DateTime.Now,
                    Status.New,
                    "Razriv",
                    new List<Comment>()
                    {
                        new Comment(new RedmineUser(321, "Jora"), DateTime.Now, "Sdelal pic"),
                        new Comment(new RedmineUser(432, "Jija"), DateTime.Now, "Jopniy")
                    }
                )
            );
        }
    }
}
