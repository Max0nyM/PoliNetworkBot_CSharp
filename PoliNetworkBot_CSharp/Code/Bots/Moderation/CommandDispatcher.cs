﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class CommandDispatcher
    {
        public static async Task CommandDispatcherMethod(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var cmdLines = e.Message.Text.Split(' ');
            var cmd = cmdLines[0];
            switch (cmd)
            {
                case "/start":
                {
                    await Start(sender, e);
                    return;
                }

                case "/force_check_invite_links":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.Chat.Id))
                        _ = ForceCheckInviteLinksAsync(sender, e);
                    else
                        await DefaultCommand(sender, e);
                    return;
                }

                case "/contact":
                {
                    await ContactUs(sender, e);
                    return;
                }

                case "/help":
                {
                    await Help(sender, e);
                    return;
                }

                case "/banAll":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.From.Id))
                        _ = BanAllAsync(sender, e, cmdLines);
                    else
                        await DefaultCommand(sender, e);
                    return;
                }

                case "/ban":
                {
                    _ = BanUserAsync(sender, e, cmdLines);
                    return;
                }

                case "/unbanAll":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.From.Id))
                        _ = UnbanAllAsync(sender, e, cmdLines[1]);
                    else
                        await DefaultCommand(sender, e);
                    return;
                }

                case "/getGroups":
                {
                    if (GlobalVariables.Creators.Contains(e.Message.From.Id) && e.Message.Chat.Type == ChatType.Private)
                    {
                        string username = null;
                        if (!string.IsNullOrEmpty(e.Message.From.Username))
                        {
                            username = e.Message.From.Username;
                        }
                        _ = GetAllGroups(e.Message.From.Id, username, sender);
                        return;
                    }
                    else
                    {
                        await DefaultCommand(sender, e);
                    }

                    return;
                }

                case "/time":
                {
                    await SendMessage.SendMessageInPrivate(sender, e, DateTimeClass.NowAsStringAmericanFormat());
                    return;
                }

                case "/assoc_send":
                {
                    _ = Assoc_SendAsync(sender, e);
                    return;
                }

                default:
                {
                    await DefaultCommand(sender, e);
                    return;
                }
            }
        }

        private static async Task<bool> GetAllGroups(long chatId, string username, TelegramBotAbstract sender)
        {
            var groups = Groups.GetAllGroups();
            Stream stream = new MemoryStream();
            FileSerialization.SerializeFile(groups, ref stream);
            TLAbsInputPeer peer2 = new TLInputPeerUser() { UserId = (int)chatId};
            var peer = new Tuple<TLAbsInputPeer, long>(peer2, chatId);
 
                        
            return await SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin", 
                    caption: null, mimeType: "application/octet-stream"), peer,
                "Here are all groups:", TextAsCaption.BEFORE_FILE,
                sender, username);
        }

        private static async Task<bool> Assoc_SendAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var replyTo = e.Message.ReplyToMessage;

            var languageList = new Language(new Dictionary<string, string>
            {
                {"it", "Scegli l'entità per il quale stai componendo il messaggio"},
                {"en", "Choose the entity you are writing this message for"}
            });

            var messageFromIdEntity = await Assoc.GetIdEntityFromPersonAsync(e.Message.From.Id, languageList,
                sender, e.Message.From.LanguageCode);

            var languageList2 = new Language(new Dictionary<string, string>
                {
                    {"it", "Data di pubblicazione?"},
                    {"en", "Date of pubblication?"}
                }
            );

            var opt1 = new Language(new Dictionary<string, string> {{"it", "Metti in coda"}, {"en", "Place in queue"}});
            var opt2 = new Language(
                new Dictionary<string, string> {{"it", "Scegli la data"}, {"en", "Choose the date"}});
            var options = new List<List<Language>>
            {
                new List<Language> {opt1, opt2}
            };

            var queueOrPreciseDate = await AskUser.AskBetweenRangeAsync(e.Message.From.Id,
                languageList2, sender, e.Message.From.LanguageCode, options);

            DateTime? sentDate = null;
            if (Language.EqualsLang(queueOrPreciseDate, options[0][0], e.Message.From.LanguageCode))
                sentDate = null;
            else
                sentDate = await DateTimeClass.AskDateAsync(e.Message.From.Id, e.Message.Text,
                    e.Message.From.LanguageCode, sender);


            const long idChatSentInto = Channels.PoliAssociazioni;

            if (replyTo.Photo != null)
            {
                var photoLarge = UtilsPhoto.GetLargest(replyTo.Photo);
                var photoIdDb = UtilsPhoto.AddPhotoToDb(photoLarge);
                if (photoIdDb == null)
                    return false;

                MessageDb.AddMessage(MessageType.Photo,
                    replyTo.Caption, e.Message.From.Id,
                    messageFromIdEntity, photoIdDb.Value,
                    idChatSentInto, sentDate, false,
                    sender.GetId(), replyTo.MessageId);
            }
            else
            {
                await sender.SendTextMessageAsync(e.Message.From.Id,
                    "You have to attach something! (A photo, for example)",
                    ChatType.Private);
                return false;
            }

            await sender.SendTextMessageAsync(e.Message.From.Id, "The message has been submitted correctly",
                ChatType.Private);
            return true;
        }

        private static async Task<bool> BanUserAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string[] stringInfo)
        {
            var r = await Groups.CheckIfAdminAsync(e.Message.From.Id, e.Message.Chat.Id, sender);
            if (!r)
                return false;

            if (e.Message.ReplyToMessage == null)
            {
                var targetInt = await Info.GetTargetUserIdAsync(stringInfo[1], sender);
                return targetInt != null &&
                       await RestrictUser.BanUserFromGroup(sender, e, targetInt.Value, e.Message.Chat.Id, null);
            }
            else
            {
                var targetInt = e.Message.ReplyToMessage.From.Id;
                return await RestrictUser.BanUserFromGroup(sender, e, targetInt, e.Message.Chat.Id, stringInfo);
            }
        }

        private static async Task UnbanAllAsync(TelegramBotAbstract sender, MessageEventArgs e, string target)
        {
            var done = await RestrictUser.BanAllAsync(sender, e, target, false);
            await SendMessage.SendMessageInPrivate(sender, e,
                "Target unbanned from " + done.Count + " groups");
        }

        private static async Task BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e,
            IReadOnlyList<string> target)
        {
            if (e.Message.ReplyToMessage == null)
            {
                if (target.Count < 2)
                {
                    await sender.SendTextMessageAsync(e.Message.From.Id, "We can't find the target.", ChatType.Private);
                }
                else
                {
                    var done = await RestrictUser.BanAllAsync(sender, e, target[1], true);
                    await SendMessage.SendMessageInPrivate(sender, e,
                        "Target banned from " + done.Count + " groups");
                }
            }
            else
            {
                var done = await RestrictUser.BanAllAsync(sender, e, e.Message.ReplyToMessage.From.Id.ToString(), true);
                await SendMessage.SendMessageInPrivate(sender, e,
                    "Target banned from " + done.Count + " groups");
            }
        }

        private static async Task DefaultCommand(TelegramBotAbstract sender, MessageEventArgs e)
        {
            await SendMessage.SendMessageInPrivate(sender, e,
                "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)");
        }

        private static async Task Help(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private)
                await HelpPrivate(sender, e);
            else
                await SendMessage.SendMessageInPrivateOrAGroup(sender, e,
                    "Questo messaggio funziona solo in chat privata");
        }

        private static async Task HelpPrivate(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "<i>Lista di funzioni</i>:\n" +
                                "\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                                "\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                                "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                "FAQ (domande frequenti)</a>\n" +
                                "\n🏫 Bot ricerca aule libere @AulePolimiBot\n" +
                                "\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                                "\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                                "\n👥 Gruppo consigliati e utili /groups\n" +
                                "\n⚠ Hai già letto le regole del network? /rules\n" +
                                "\n✍ Per contattarci /contact";
            await SendMessage.SendMessageInPrivate(sender, e, text, ParseMode.Html);
        }

        private static async Task ContactUs(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                telegramBotClient.GetContactString(), e.Message.Chat.Type
            );
        }

        private static async Task ForceCheckInviteLinksAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var n = await InviteLinks.FillMissingLinksIntoDB_Async(sender);
            await SendMessage.SendMessageInPrivate(sender, e, "I have updated n=" + n + " links");
        }

        private static async Task Start(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                "Ciao! 👋\n" +
                "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                "\nVisita anche il nostro sito " + telegramBotClient.GetWebSite(),
                e.Message.Chat.Type
            );
        }
    }
}