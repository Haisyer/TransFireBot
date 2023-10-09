using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PKHeX.Core;
using SysBot.Base;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.Dodo
{
    public class DodoHelper<T> : PokemonTradeHelper<T> where T : PKM, new()
    {
        private readonly string channelId = default!;
        private readonly string islandSourceId = default!;

        public DodoHelper(ulong dodoId, string nickName, string channelId, string islandSourceId)
        {
            SetPokeTradeTrainerInfo(new PokeTradeTrainerInfo(nickName, dodoId));
            SetTradeQueueInfo(DodoBot<T>.Info);
            this.channelId = channelId;
            this.islandSourceId = islandSourceId;
            
        }

        public override IPokeTradeNotifier<T> GetPokeTradeNotifier(T pkm, int code)
        {
            return new DodoTradeNotifier<T>(pkm, userInfo, code, userInfo.ID.ToString(), channelId, islandSourceId);
        }

        public override void SendMessage(string message)
        {
            DodoBot<T>.SendChannelAtMessage(userInfo.ID, message, channelId);
        }

        public override void SendCardMessage(string message, string pokeurl,string itemurl, string ballurl,string teraurl, string teraoriginalurl, string shinyurl, string movetypeurl1, string movetypeurl2, string movetypeurl3, string movetypeurl4)
        {
            DodoBot<T>.SendChannelCardMessage(message, channelId, pokeurl,itemurl,ballurl,teraurl,teraoriginalurl, shinyurl, movetypeurl1, movetypeurl2, movetypeurl3, movetypeurl4);
        }

        public override void SendCardBatchMessage(string message, string pokeurl, string itemurl, string ballurl, string shinyurl)
        {
            DodoBot<T>.SendChannelCardBatchMessage(message, channelId, pokeurl, itemurl, ballurl, shinyurl);
        }

    }
}
