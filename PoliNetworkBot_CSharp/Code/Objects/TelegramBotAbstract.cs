﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class TelegramBotAbstract
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _contactString;
        private readonly int _id;
        private readonly BotTypeApi _isbot;
        private readonly TelegramClient _userbotClient;

        private readonly string _website;

        public TelegramBotAbstract(TelegramBotClient botClient, string website, string contactString,
            BotTypeApi botTypeApi)
        {
            _botClient = botClient;
            _isbot = botTypeApi;
            _website = website;
            _contactString = contactString;
        }

        public TelegramBotAbstract(TelegramClient userbotClient, string website, string contactString, long id,
            BotTypeApi botTypeApi)
        {
            _userbotClient = userbotClient;
            _isbot = botTypeApi;
            _website = website;
            _contactString = contactString;
            _id = (int) id;
        }

        internal string GetWebSite()
        {
            return _website;
        }

        internal static TelegramBotAbstract GetFromRam(TelegramBotClient telegramBotClientBot)
        {
            return GlobalVariables.Bots[telegramBotClientBot.BotId];
        }

        internal async Task DeleteMessageAsync(long chatId, int messageId, ChatType chatType)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    await _botClient.DeleteMessageAsync(chatId, messageId);
                    break;
                case BotTypeApi.USER_BOT:
                    await _userbotClient.ChannelsDeleteMessageAsync(UserbotPeer.GetPeerChannelFromIdAndType(chatId),
                        new TLVector<int> {messageId});
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal async Task<int?> GetIdFromUsernameAsync(string target)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return null; //bot api does not allow that
                case BotTypeApi.USER_BOT:
                    var r = await _userbotClient.ResolveUsernameAsync(target);
                    return r.Peer switch
                    {
                        null => null,
                        TLPeerUser tLPeerUser => tLPeerUser.UserId,
                        _ => null
                    };
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        internal async Task RestrictChatMemberAsync(long chatId, int userId, ChatPermissions permissions,
            DateTime untilDate)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    await _botClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate);
                    break;
                case BotTypeApi.USER_BOT:
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal int GetId()
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return _botClient.BotId;
                case BotTypeApi.USER_BOT:
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return _id;
        }

        internal async Task<bool> SendTextMessageAsync(long chatid, Language text,
            ChatType chatType, string lang, ParseMode parseMode,
            ReplyMarkupObject replyMarkupObject, string username)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    IReplyMarkup reply = null;
                    if (replyMarkupObject != null) reply = replyMarkupObject.GetReplyMarkupBot();

                    var m1 = await _botClient.SendTextMessageAsync(chatid, text.Select(lang), parseMode,
                        replyMarkup: reply);
                    return m1 != null;
                case BotTypeApi.USER_BOT:
                case BotTypeApi.DISGUISED_BOT:
                    var peer = UserbotPeer.GetPeerFromIdAndType(chatid, chatType);
                    try
                    {
                        TLAbsReplyMarkup replyMarkup = null;
                        if (replyMarkupObject != null) replyMarkup = replyMarkupObject.GetReplyMarkupUserBot();
                        var m3 = await SendMessage.SendMessageUserBot(_userbotClient,
                            peer, text, username, replyMarkup, lang);
                        return m3 != null;
                    }
                    catch (Exception e)
                    {
                        ;
                    }

                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<bool> SendMedia(GenericFile genericFile, long chatid, ChatType chatType, string username,
            Language caption, string lang)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                {
                    var messageType = genericFile.GetMediaBotType();
                    switch (messageType)
                    {
                        case MessageType.Unknown:
                            break;
                        case MessageType.Text:
                            break;
                        case MessageType.Photo:
                            break;
                        case MessageType.Audio:
                            break;
                        case MessageType.Video:
                            break;
                        case MessageType.Voice:
                            break;
                        case MessageType.Document:
                            break;
                        case MessageType.Sticker:
                            break;
                        case MessageType.Location:
                            break;
                        case MessageType.Contact:
                            break;
                        case MessageType.Venue:
                            break;
                        case MessageType.Game:
                            break;
                        case MessageType.VideoNote:
                            break;
                        case MessageType.Invoice:
                            break;
                        case MessageType.SuccessfulPayment:
                            break;
                        case MessageType.WebsiteConnected:
                            break;
                        case MessageType.ChatMembersAdded:
                            break;
                        case MessageType.ChatMemberLeft:
                            break;
                        case MessageType.ChatTitleChanged:
                            break;
                        case MessageType.ChatPhotoChanged:
                            break;
                        case MessageType.MessagePinned:
                            break;
                        case MessageType.ChatPhotoDeleted:
                            break;
                        case MessageType.GroupCreated:
                            break;
                        case MessageType.SupergroupCreated:
                            break;
                        case MessageType.ChannelCreated:
                            break;
                        case MessageType.MigratedToSupergroup:
                            break;
                        case MessageType.MigratedFromGroup:
                            break;
                        case MessageType.Animation:
                            break;
                        case MessageType.Poll:
                            break;
                        case MessageType.Dice:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
                case BotTypeApi.USER_BOT:
                {
                    var peer = UserbotPeer.GetPeerFromIdAndType(chatid, chatType);
                    var media2 = await genericFile.GetMediaTl(_userbotClient);

                    var r = await media2.SendMedia(peer, _userbotClient, caption, username, lang);
                    return r != null;

                    break;
                }
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<bool> SendFileAsync(TelegramFile documentInput, Tuple<TLAbsInputPeer, long> peer,
            Language text,
            TextAsCaption textAsCaption, string username, string lang)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    var inputOnlineFile = documentInput.GetOnlineFile();
                    switch (textAsCaption)
                    {
                        case TextAsCaption.AS_CAPTION:
                        {
                            _ = await _botClient.SendDocumentAsync(peer.Item2, inputOnlineFile, text.Select(lang));
                            return true;
                        }

                        case TextAsCaption.BEFORE_FILE:
                        {
                            _ = await _botClient.SendTextMessageAsync(peer.Item2, text.Select(lang));
                            _ = await _botClient.SendDocumentAsync(peer.Item2, inputOnlineFile);
                            return true;
                        }

                        case TextAsCaption.AFTER_FILE:
                        {
                            _ = await _botClient.SendDocumentAsync(peer.Item2, inputOnlineFile);
                            _ = await _botClient.SendTextMessageAsync(peer.Item2, text.Select(lang));
                            return true;
                        }

                        default:
                            throw new ArgumentOutOfRangeException(nameof(textAsCaption), textAsCaption, null);
                    }


                    return false;

                case BotTypeApi.USER_BOT:
                    switch (textAsCaption)
                    {
                        case TextAsCaption.AS_CAPTION:
                        {
                            var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                            var r = await tlFileToSend.SendMedia(peer.Item1, _userbotClient, text, username, lang);
                            return r != null;
                        }

                        case TextAsCaption.BEFORE_FILE:
                        {
                            var r2 = await SendMessage.SendMessageUserBot(_userbotClient, peer.Item1, text, username,
                                new TLReplyKeyboardHide(), lang);
                            var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                            var r = await tlFileToSend.SendMedia(peer.Item1, _userbotClient, null, username, lang);
                            return r != null && r2 != null;
                        }

                        case TextAsCaption.AFTER_FILE:
                        {
                            var tlFileToSend = await documentInput.GetMediaTl(_userbotClient);
                            var r = await tlFileToSend.SendMedia(peer.Item1, _userbotClient, null, username, lang);
                            var r2 = await SendMessage.SendMessageUserBot(_userbotClient, peer.Item1, text, username,
                                new TLReplyKeyboardHide(), lang);
                            return r != null && r2 != null;
                        }

                        default:
                            throw new ArgumentOutOfRangeException(nameof(textAsCaption), textAsCaption, null);
                    }

                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }


        internal string GetContactString()
        {
            return _contactString;
        }

        internal async Task<bool> IsAdminAsync(int userId, long chatId)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    var admins = await _botClient.GetChatAdministratorsAsync(chatId);
                    return admins.Any(admin => admin.User.Id == userId);
                    ;
                case BotTypeApi.USER_BOT:
                    var r = await _userbotClient.ChannelsGetParticipant(
                        UserbotPeer.GetPeerChannelFromIdAndType(chatId),
                        UserbotPeer.GetPeerUserFromdId(userId));

                    return r.Participant is TLChannelParticipantModerator ||
                           r.Participant is TLChannelParticipantCreator;
                    ;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<string> ExportChatInviteLinkAsync(long chatId)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return await _botClient.ExportChatInviteLinkAsync(chatId);
                    ;
                case BotTypeApi.USER_BOT:
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        internal async Task<bool> SendMessageReactionAsync(int chatId, string emojiReaction, int messageId,
            ChatType chatType)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    break;
                case BotTypeApi.USER_BOT:
                    /*
                    var updates =
                        await _userbotClient.SendMessageReactionAsync(
                            UserbotPeer.GetPeerFromIdAndType(chatId, chatType),
                            messageId, emojiReaction);
                    return updates != null;
                    */
                    break;
                    ;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<bool> BanUserFromGroup(int target, long groupChatId, MessageEventArgs e, string[] time)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:

                    var untilDate = DateTimeClass.GetUntilDate(time);

                    try
                    {
                        if (untilDate == null)
                        {
                            await _botClient.KickChatMemberAsync(groupChatId, target);
                            return true;
                        }

                        await _botClient.KickChatMemberAsync(groupChatId, target, untilDate.Value);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }

                    return true;
                    ;
                case BotTypeApi.USER_BOT:
                    return false;
                case BotTypeApi.DISGUISED_BOT:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<TLAbsDialogs> GetLastDialogsAsync()
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return null;
                case BotTypeApi.USER_BOT:
                    return await _userbotClient.GetUserDialogsAsync(limit: 100);
                    ;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        internal async Task<ChatMember[]> GetChatAdministratorsAsync(long id)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return await _botClient.GetChatAdministratorsAsync(id);
                case BotTypeApi.USER_BOT:
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        internal async Task<bool> UnBanUserFromGroup(int target, long groupChatId, MessageEventArgs e)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    try
                    {
                        await _botClient.UnbanChatMemberAsync(groupChatId, target);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }

                    ;
                case BotTypeApi.USER_BOT:
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        internal async Task<bool> LeaveChatAsync(long id)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    await _botClient.LeaveChatAsync(id);
                    return true;
                case BotTypeApi.USER_BOT:
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public async Task<bool> SendPhotoAsync(long chatIdToSendTo, ObjectPhoto objectPhoto, string caption,
            ParseMode parseMode, ChatType chatTypeToSendTo)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    var m1 = await _botClient.SendPhotoAsync(chatIdToSendTo,
                        objectPhoto.GetTelegramBotInputOnlineFile(), caption, parseMode);
                    return m1 != null;
                    ;
                case BotTypeApi.USER_BOT:

                    var photoFile = await objectPhoto.GetTelegramUserBotInputPhoto(_userbotClient);
                    if (photoFile == null)
                        return false;

                    var m2 = await _userbotClient.SendUploadedPhoto(
                        UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, chatTypeToSendTo), caption: caption,
                        file: photoFile);
                    return m2 != null;
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return false;
        }

        public async Task<bool> CreateGroup(string name, string description, IEnumerable<long> membersToInvite)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    return false;
                case BotTypeApi.USER_BOT:
                    break;
                case BotTypeApi.DISGUISED_BOT:
                    var users = new TLVector<TLAbsInputUser>();
                    foreach (var userId in membersToInvite) users.Add(new TLInputUser {UserId = (int) userId});

                    try
                    {
                        var r = await _userbotClient.Messages_CreateChat(name, users);
                        return r != null;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public async Task<bool> SendVideoAsync(long chatIdToSendTo, ObjectVideo video, string caption,
            ParseMode parseMode, ChatType chatTypeToSendTo)
        {
            switch (_isbot)
            {
                case BotTypeApi.REAL_BOT:
                    try
                    {
                        var m1 = await _botClient.SendVideoAsync(chatIdToSendTo, caption: caption,
                            video: video.GetTelegramBotInputOnlineFile(), duration: video.GetDuration(),
                            height: video.GetHeight(),
                            width: video.GetWidth(), parseMode: parseMode);
                        return m1 != null;
                    }
                    catch
                    {
                        return false;
                    }

                case BotTypeApi.USER_BOT:
                {
                    var videoFile = await video.GetTelegramUserBotInputVideo(_userbotClient);
                    if (videoFile == null)
                        return false;

                    //UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, ChatType.Private), videoFile, caption
                    var media2 = video.GetTLabsInputMedia();
                    var m2 = await _userbotClient.Messages_SendMedia(
                        UserbotPeer.GetPeerFromIdAndType(chatIdToSendTo, chatTypeToSendTo), media2);
                    return m2 != null;
                }
                case BotTypeApi.DISGUISED_BOT:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return false;
        }
    }
}