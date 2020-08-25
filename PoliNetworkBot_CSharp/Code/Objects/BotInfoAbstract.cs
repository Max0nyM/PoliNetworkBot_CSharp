﻿#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    public class BotInfoAbstract
    {
        protected readonly Dictionary<string, object> KeyValuePairs;

        public BotInfoAbstract()
        {
            KeyValuePairs = new Dictionary<string, object>();
        }

        internal bool IsBot()
        {
            return (bool) KeyValuePairs[ConstConfigBot.IsBot];
        }

        internal bool SetIsBot(bool v)
        {
            KeyValuePairs[ConstConfigBot.IsBot] = v;
            return true;
        }

        internal string GetToken()
        {
            return KeyValuePairs[ConstConfigBot.Token].ToString();
        }

        internal void SetWebsite(string v)
        {
            KeyValuePairs[ConstConfigBot.Website] = v;
        }

        internal void SetContactString(string v)
        {
            KeyValuePairs[ConstConfigBot.ContactString] = v;
        }

        internal void SetOnMessages(string v)
        {
            KeyValuePairs[ConstConfigBot.OnMessages] = v;
        }

        internal void SetAcceptMessages(bool v)
        {
            KeyValuePairs[ConstConfigBot.AcceptsMessages] = v;
        }

        internal void SetToken(string v)
        {
            KeyValuePairs[ConstConfigBot.Token] = v;
        }

        internal EventHandler<MessageEventArgs> GetOnMessage()
        {
            var s = KeyValuePairs[ConstConfigBot.OnMessages].ToString();
            return BotStartMethods.GetMethodFromString(s);
        }

        internal bool AcceptsMessages()
        {
            return (bool) KeyValuePairs[ConstConfigBot.AcceptsMessages];
        }

        internal string GetWebsite()
        {
            try
            {
                return KeyValuePairs[ConstConfigBot.Website].ToString();
            }
            catch
            {
                return null;
            }
        }

        internal string GetContactString()
        {
            try
            {
                return KeyValuePairs[ConstConfigBot.ContactString].ToString();
            }
            catch
            {
                return null;
            }
        }

        public class ConstConfigBot
        {
            public const string Token = "t";
            public const string IsBot = "b";
            public const string AcceptsMessages = "a";
            public const string OnMessages = "o";
            public const string Website = "w";
            public const string ContactString = "c";
            public const string ApiId = "ai";
            public const string ApiHash = "ah";
            public const string UserId = "u";
            public const string NumberCountry = "nc";
            public const string NumberNumber = "nn";
            public const string PasswordToAuthenticate = "pta";
        }
    }
}