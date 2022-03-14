using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace RedmineTelegram
{
    sealed public class TelegramBot
    {
        private TelegramBotClient _bot;
        //Token for bot Jijoba @jijoba_bot:
        private string Token => "2098827232:AAFu37Kco2dtw0vFRkNo0DYqKww68hY5Dh0";

        public TelegramBot()
        {
            _bot = new TelegramBotClient(Token);
            _bot.OnMessage += OnMessageHandler;
            _bot.OnCallbackQuery += OnButtonClick;
        }

        public void SendNewTaskMessageToUser()
        {

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
            string text = e.Message.Text;
            if (e.Message.Text == "/start")
            {

            }
        }

        private async void OnButtonClick(object sender, CallbackQueryEventArgs e)
        {
            var callbackData = e.CallbackQuery.Data;
            long taskId = long.Parse(callbackData[1..]);
            if (callbackData[0] == 'c')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст комментария.");
            }
            if (callbackData[0] == 'd')
            {
                await _bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Введите текст статуса.");
            }
        }
    }
}
