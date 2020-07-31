﻿using System;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClient = null;
            if (sender is TelegramBotClient tmp)
            {
                telegramBotClient = tmp;
            }

            if (telegramBotClient == null)
                return;

            bool to_exit = CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (to_exit)
            {
                ExitFromChat(telegramBotClient, e);
                return;
            }

            bool check_username = CheckUsername(telegramBotClient, e);
            if (check_username)
            {
                SendUsernameWarning(telegramBotClient, e);
                return;
            }

            SpamType check_spam = CheckSpam(telegramBotClient, e);
            if (check_spam != SpamType.ALL_GOOD)
            {
                AntiSpamMeasure(telegramBotClient, e, check_spam);
                return;
            }

            if (!string.IsNullOrEmpty(e.Message.Text))
            {
                if (e.Message.Text.StartsWith("/"))
                {
                    CommandDispatcher(telegramBotClient, e);
                }
            }


        }

        private static void AntiSpamMeasure(TelegramBotClient telegramBotClient, MessageEventArgs e, SpamType check_spam)
        {
            throw new NotImplementedException();
        }

        private static SpamType CheckSpam(TelegramBotClient telegramBotClient, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty( e.Message.Text))
            {
                //todo
                return SpamType.ALL_GOOD;
            }


            if (e.Message.Text.StartsWith("/"))
                return SpamType.ALL_GOOD;

            return Moderation.Blacklist.IsSpam(e.Message.Text);
        }

        private static void SendUsernameWarning(TelegramBotClient telegramBotClient, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static bool CheckUsername(TelegramBotClient telegramBotClient, MessageEventArgs e)
        {
            if ( string.IsNullOrEmpty(e.Message.From.Username))
            {
                return true;
            }

            if (e.Message.From.FirstName.Length < 2)
                return true;

            return false;
        }

        private static void ExitFromChat(TelegramBotClient sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static bool CheckIfToExitAndUpdateGroupList(TelegramBotClient sender, MessageEventArgs e)
        {
            switch (e.Message.Chat.Type)
            {
                case Telegram.Bot.Types.Enums.ChatType.Private:
                    return false;
            }

            string q1 = "SELECT id, valid FROM Groups WHERE id = @id";
            var dt = Utils.SQLite.ExecuteSelect(q1, new System.Collections.Generic.Dictionary<string, object>() { { "@id", e.Message.Chat.Id } });
            if (dt != null && dt.Rows.Count > 0)
            {
                return CheckIfToExit(sender, e, dt.Rows[0].ItemArray[1]);
            }
            else
            {
                InsertGroup(sender, e);
                return CheckIfToExit(sender, e, null);
            }

        }

        private static void InsertGroup(TelegramBotClient sender, MessageEventArgs e)
        {
            string q1 = "INSERT INTO Groups (id, bot_id) VALUES (@id, @botid)";
            Utils.SQLite.Execute(q1, new System.Collections.Generic.Dictionary<string, object>() { {"@id", e.Message.Chat.Id }, { "@botid", sender.BotId } });
        }

        private static bool CheckIfToExit(TelegramBotClient sender, MessageEventArgs e, object v)
        {
            if (v == null || v is System.DBNull)
            {
                //todo: check if admins are allowed and set valid column
            }
            else if (v is bool b)
            {
                return !b;
            }
            else
            {
                throw new NotImplementedException();
            }

            return false;
        }

        private static void CommandDispatcher(TelegramBotClient sender, MessageEventArgs e)
        {
            switch(e.Message.Text)
            {
                case "/start":
                    {
                        Start(sender, e);
                        return;
                    }
            }
        }

        private static void Start(TelegramBotClient telegramBotClient, MessageEventArgs e)
        {
            telegramBotClient.SendTextMessageAsync(e.Message.From.Id, "Ciao!");     
        }
    }
}
