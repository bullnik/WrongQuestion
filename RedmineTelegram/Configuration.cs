using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class Configuration
    {
        public readonly string RedmineDatabaseHost = "localhost";
        public readonly int RedmineDatabasePort = 3307;
        public readonly string RedmineDatabaseName = "";
        public readonly string RedmineDatabaseUsername = "admindb";
        public readonly string RedmineDatabasePassword = "password123";
        //Token for bot Jijoba @jijoba_bot:
        public readonly string TelegramBotToken = "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";

        public static bool TryGetFromJson(string filename, out Configuration configuration)
        {
            try
            {
                configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(filename));
            }
            catch
            {
                configuration = null;
            }
            if (configuration != null)
            {
                return true;
            }
            return false;
        }

        public void WriteToJson(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this));
        }

        public Configuration()
        {

        }
    }
}
