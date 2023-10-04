using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Manganese.Array;
using Mirai.Net.Utils.Scaffolds;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQHelper<T> : PokemonTradeHelper<T> where T : PKM, new()
    {
        private readonly string GroupId = default!;
        internal static LegalitySettings set = default!;
        public MiraiQQHelper(string qq, string nickName) 
        {        
            SetPokeTradeTrainerInfo(new PokeTradeTrainerInfo(nickName, ulong.Parse(qq)));
            SetTradeQueueInfo(MiraiQQBot<T>.Info);
            GroupId = MiraiQQBot<T>.Settings.GroupId;
        }

        public override IPokeTradeNotifier<T> GetPokeTradeNotifier(T pkm, int code)
        {
            return new MiraiQQTradeNotifier<T>(pkm, userInfo, code, userInfo.TrainerName, GroupId);
        }

        public override void SendMessage(string message)
        {
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(userInfo.ID.ToString()).Plain(message).Build());
        }

        #region
        // QQ没有卡片信息,后续可以改造成图片消息
        public override void SendCardMessage(string message, string pokeurl, string itemurl, string ballurl, string teraurl, string teraoriginalurl)
        {
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(userInfo.ID.ToString()).Plain(message).Build());
        }
        //  QQ没有卡片信息,后续可以改造成图片消息
        public override void SendCardBatchMessage(string message, string pokeurl, string itemurl, string ballurl)
        {
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(userInfo.ID.ToString()).Plain(message).Build());
        }
        #endregion
    }
}