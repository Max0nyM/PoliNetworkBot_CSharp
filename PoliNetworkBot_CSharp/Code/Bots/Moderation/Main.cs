﻿#region

using System.Threading;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => MainMethod2(sender, e));
            t.Start();
        }

        private static void MainMethod2(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClientBot = null;
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

            if (telegramBotClientBot == null)
                return;

            var telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

            var toExit = ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (toExit)
            {
                LeaveChat.ExitFromChat(telegramBotClient, e);
                return;
            }

            var check_username = ModerationCheck.CheckUsername(e);
            if (check_username.Item1 || check_username.Item2)
            {
                ModerationCheck.SendUsernameWarning(telegramBotClient, e, check_username.Item1, check_username.Item2);
                return;
            }

            var checkSpam = ModerationCheck.CheckSpam(e);
            if (checkSpam != SpamType.ALL_GOOD)
            {
                ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);
                return;
            }

            if (string.IsNullOrEmpty(e.Message.Text))
                return;
            
            if (e.Message.Text.StartsWith("/"))
                CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
            else
                TextConversation.DetectMessage(telegramBotClient, e);
        }
    }
}