﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class ModerationCheck
    {
        public static async Task<bool> CheckIfToExitAndUpdateGroupList(TelegramBotAbstract sender, MessageEventArgs e)
        {
            switch (e.Message.Chat.Type)
            {
                case ChatType.Private:
                    return false;
                case ChatType.Group:
                    break;
                case ChatType.Channel:
                    break;
                case ChatType.Supergroup:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            const string q1 = "SELECT id, valid FROM Groups WHERE id = @id";
            var dt = SqLite.ExecuteSelect(q1, new Dictionary<string, object> {{"@id", e.Message.Chat.Id}});
            if (dt != null && dt.Rows.Count > 0) return await CheckIfToExit(sender, e, dt.Rows[0].ItemArray[1]);

            InsertGroup(sender, e);
            return await CheckIfToExit(sender, e, null);
        }

        private static async Task<bool> CheckIfToExit(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            object v)
        {
            switch (v)
            {
                case null:
                case DBNull _:
                    return await CheckIfToExit_NullValue(telegramBotClient, e);
                case char b:
                    return b != 'Y';
                case string s when string.IsNullOrEmpty(s):
                    return await CheckIfToExit_NullValue(telegramBotClient, e);
                case string s:
                    return s != "Y";
                default:
                    return await CheckIfToExit_NullValue(telegramBotClient, e);
            }
        }

        private static async Task<bool> CheckIfToExit_NullValue(TelegramBotAbstract telegramBotClient,
            MessageEventArgs e)
        {
            var r = await telegramBotClient.GetChatAdministratorsAsync(e.Message.Chat.Id);

            //todo: check if admins are allowed and set valid column
            return false;
        }

        private static void InsertGroup(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string q1 = "INSERT INTO Groups (id, bot_id, type, title) VALUES (@id, @botid, @type, @title)";
            SqLite.Execute(q1, new Dictionary<string, object>
            {
                {"@id", e.Message.Chat.Id},
                {"@botid", sender.GetId()},
                {"@type", e.Message.Chat.Type.ToString()},
                {"@title", e.Message.Chat.Title}
            });
            _ = CreateInviteLinkAsync(sender, e);
        }

        private static async Task<bool> CreateInviteLinkAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            return await InviteLinks.CreateInviteLinkAsync(e.Message.Chat.Id, sender);
        }

        private static List<UsernameAndNameCheckResult> CheckUsername(MessageEventArgs e)
        {
            var r = new List<UsernameAndNameCheckResult>
            {
                CheckUsername2(e.Message.From.Username, e.Message.From.FirstName,
                    lastName: e.Message.From.LastName, language: e.Message.From.LanguageCode, userId: e.Message.From.Id)
            };

            if (e.Message.NewChatMembers == null)
                return r;

            r.AddRange(from user in e.Message.NewChatMembers
                where user != null
                select CheckUsername2(user.Username, user.FirstName, user.Id,
                    user.LastName, user.LanguageCode));

            return r;
        }

        private static UsernameAndNameCheckResult CheckUsername2(string fromUsername, string fromFirstName, int userId,
            string lastName, string language)
        {
            var username = false;
            var name = false;

            if (string.IsNullOrEmpty(fromUsername)) username = true;

            if (fromFirstName.Length < 2)
                name = true;

            return new UsernameAndNameCheckResult(username, name, language,
                fromUsername, userId, fromFirstName, lastName);
        }

        public static SpamType CheckSpam(MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.Text))
                //todo
                return SpamType.ALL_GOOD;

            if (e.Message.Text.StartsWith("/"))
                return SpamType.ALL_GOOD;

            var isForeign = DetectForeignLanguage(e);

            return isForeign ? SpamType.FOREIGN : Blacklist.IsSpam(e.Message.Text);
        }

        private static bool DetectForeignLanguage(MessageEventArgs e)
        {
            if (e.Message.Chat.Id == -1001394018284)
                return false;

            return Regex.Match(e.Message.Text, "[\uac00-\ud7a3]").Success ||
                   Regex.Match(e.Message.Text, "[\u3040-\u30ff]").Success ||
                   Regex.Match(e.Message.Text, "[\u4e00-\u9FFF]").Success;
        }

        private static async Task SendUsernameWarning(TelegramBotAbstract telegramBotClient,
            bool username, bool name, string lang, string usernameOfUser,
            long chatId, int userId, int? messageId, ChatType messageChatType,
            string firstName, string lastName)
        {
            var s1I = "Imposta un username e un nome più lungo dalle impostazioni di telegram\n";
            if (username && !name)
                s1I = "Imposta un username dalle impostazioni di telegram\n";
            else if (!username && name)
                s1I = "Imposta un nome più lungo " +
                      "dalle impostazioni di telegram\n";


            var s1E = "Set an username and a longer first name from telegram settings";
            if (username && !name)
                s1E = "Set an username from telegram settings";
            else if (!username && name)
                s1E = "Set a longer first name from telegram settings";


            var s2 = new Language(new Dictionary<string, string>
            {
                {"it", s1I},
                {"en", s1E}
            });
            await SendMessage.SendMessageInPrivateOrAGroup(telegramBotClient, s2, lang,
                usernameOfUser, userId, firstName, lastName, chatId, messageChatType);
            await RestrictUser.Mute(60 * 5, telegramBotClient, chatId, userId);
            if (messageId != null) await telegramBotClient.DeleteMessageAsync(chatId, messageId.Value, messageChatType);
        }

        public static async Task AntiSpamMeasure(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            SpamType checkSpam)
        {
            if (checkSpam == SpamType.ALL_GOOD)
                return;

            await RestrictUser.Mute(60 * 5, telegramBotClient, e.Message.Chat.Id, e.Message.From.Id);
            var language = e.Message.From.LanguageCode.ToLower();
            switch (checkSpam)
            {
                case SpamType.SPAM_LINK:
                {
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "You sent a message with spam, and you were muted for 5 minutes"},
                        {"it", "Hai inviato un messaggio con spam, e quindi il bot ti ha mutato per 5 minuti"}
                    });


                    await SendMessage.SendMessageInPrivate(telegramBotClient, e, text2);
                    break;
                }
                case SpamType.NOT_ALLOWED_WORDS:
                {
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "You sent a message with banned words, and you were muted for 5 minutes"},
                        {"it", "Hai inviato un messaggio con parole bandite, e quindi il bot ti ha mutato per 5 minuti"}
                    });

                    await SendMessage.SendMessageInPrivate(telegramBotClient, e, text2);
                    break;
                }

                case SpamType.FOREIGN:
                {
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "You sent a message with banned characters, and you were muted for 5 minutes"},
                        {
                            "it",
                            "Hai inviato un messaggio con caratteri banditi, e quindi il bot ti ha mutato per 5 minuti"
                        }
                    });

                    await SendMessage.SendMessageInPrivate(telegramBotClient, e, text2);
                    break;
                }

                // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                case SpamType.ALL_GOOD:
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(checkSpam), checkSpam, null);
            }

            await telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, e.Message.Chat.Type);
        }

        public static async Task<bool> CheckUsernameAndName(MessageEventArgs e, TelegramBotAbstract telegramBotClient)
        {
            var usernameCheck = CheckUsername(e);
            if (usernameCheck == null)
            {
                ;
            }
            else if (usernameCheck.Count == 1)
            {
                if (!usernameCheck[0].UsernameBool && !usernameCheck[0].Name)
                    return false;

                await SendUsernameWarning(telegramBotClient, usernameCheck[0].UsernameBool,
                    usernameCheck[0].Name, e.Message.From.LanguageCode,
                    e.Message.From.Username, e.Message.Chat.Id,
                    e.Message.From.Id, e.Message.MessageId,
                    e.Message.Chat.Type, e.Message.From.FirstName,
                    e.Message.From.LastName);
                return true;
            }
            else
            {
                foreach (var usernameCheck2 in usernameCheck)
                    if (usernameCheck2 != null)
                        if (usernameCheck2.Name || usernameCheck2.UsernameBool)
                            await SendUsernameWarning(telegramBotClient,
                                usernameCheck2.UsernameBool,
                                usernameCheck2.Name,
                                usernameCheck2.GetLanguage(),
                                usernameCheck2.GetUsername(),
                                e.Message.Chat.Id,
                                usernameCheck2.GetUserId(),
                                null,
                                e.Message.Chat.Type,
                                usernameCheck2.GetFirstName(),
                                usernameCheck2.GetLastName());

                return true;
            }

            return false;
        }
    }
}