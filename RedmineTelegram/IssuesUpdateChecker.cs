using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class IssuesUpdateChecker
    {
        private readonly InternalDatabase _internalDatabase;
        private readonly RedmineDatabase _redmineDatabase;

        public IssuesUpdateChecker(InternalDatabase internalDatabase, RedmineDatabase redmineDatabase)
        {
            _internalDatabase = internalDatabase;
            _redmineDatabase = redmineDatabase;
        }

        public async void StartChecking()
        {
            await Task.Run(() => Check());
        }

        private void Check()
        {
            while (true)
            {
                Thread.Sleep(2500);
            }
        }
    }
}
