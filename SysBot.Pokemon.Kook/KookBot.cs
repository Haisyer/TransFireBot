using Kook.Rest;
using Kook;
using Kook.WebSocket;
using NLog.Fluent;
using PKHeX.Core;
using System;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.Kook
{
    //偷抄老E
    public class KookBot<T> where T : PKM, new()
    {
        private static PokeTradeHub<T> Hub = default!;
        internal static TradeQueueInfo<T> Info => Hub.Queues.Info;

        private KookSocketClient _client;
        private KookSettings Settings;

        public KookBot(KookSettings settings, PokeTradeHub<T> hub)
        {
            Hub = hub;
            Settings = settings;

            _client = new KookSocketClient();

            _client.Log += message =>
            {
                LogUtil.LogText($"{message}");
                return Task.CompletedTask;
            };
            Task.Run(async () =>
            {
                await _client.LoginAsync(TokenType.Bot, Settings.Token);
                await _client.StartAsync();
                _client.Connected += () => 
                {
                    LogUtil.LogInfo("Kook Bot is connected!", nameof(KookBot<T>));
                    return Task.CompletedTask;
                };
              //_client.MessageReceived += 
            });
            
        }

    }
}
