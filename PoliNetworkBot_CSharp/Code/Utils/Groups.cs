﻿#region

using System.Data;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Groups
    {
        internal static DataTable GetAllGroups()
        {
            const string q1 = "SELECT * FROM Groups";
            return SqLite.ExecuteSelect(q1);
        }

        internal static async Task<bool> CheckIfAdminAsync(int userId, long chatId,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (GlobalVariables.Creators.Contains(userId))
                return true;

            return await telegramBotAbstract.IsAdminAsync(userId, chatId);
        }
    }
}