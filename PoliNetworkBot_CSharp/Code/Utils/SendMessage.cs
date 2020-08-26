﻿#region

using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class SendMessage
    {
        internal static async Task<bool> SendMessageInPrivateOrAGroup(TelegramBotAbstract telegramBotClient,
            MessageEventArgs e,
            string text)
        {
            try
            {
                var r = await telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                    ParseMode.Html);
                if (r) return true;
            }
            catch
            {
                // ignored
            }

            var messageTo = GetMessageTo(e);
            var messageFor = "Messaggio per";
            var language = e.Message.From.LanguageCode.ToLower();
            messageFor = language switch
            {
                "en" => "Message for",
                _ => messageFor
            };

            var text2 = "[" + messageFor + " " + messageTo + "]\n\n" + text;
            return await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, text2, e.Message.Chat.Type,
                ParseMode.Html);
        }

        private static string GetMessageTo(MessageEventArgs e)
        {
            var name = e.Message.From.FirstName + " " + e.Message.From.LastName;
            name = name.Trim();
            return "<a href=\"tg://user?id=" + e.Message.From.Id + "\">" + name + "</a>";
        }

        internal static async Task<bool> SendMessageInPrivate(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            string text, ParseMode html = ParseMode.Default)
        {
            try
            {
                return await telegramBotClient.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, html);
            }
            catch
            {
                return false;
            }
        }

        internal static async Task<bool> SendFileAsync(TelegramFile file, long chatId,
            string text, TextAsCaption textAsCaption, TelegramBotAbstract telegramBotAbstract)
        {
            return await telegramBotAbstract.SendFileAsync(file, chatId, text, textAsCaption);
        }
    }
}