using PKHeX.Core;

using SysBot.Pokemon.QQ;

namespace SysBot.Pokemon.ConsoleApp
{
    /// <summary>
    /// Bot Environment implementation with Integrations added.
    /// </summary>
    public class PokeBotRunnerImpl<T> : PokeBotRunner<T> where T : PKM, new()
    {
        public PokeBotRunnerImpl(PokeTradeHub<T> hub, BotFactory<T> fac) : base(hub, fac) { }
        public PokeBotRunnerImpl(PokeTradeHubConfig config, BotFactory<T> fac) : base(config, fac) { }

       
        private MiraiQQBot<T>? QQ;
 

        protected override void AddIntegrations()
        {
            
            //add qq bot
            AddQQBot(Hub.Config.QQ);
           
        }

       

      

        private void AddQQBot(QQSettings config)
        {
            if (string.IsNullOrWhiteSpace(config.VerifyKey) || string.IsNullOrWhiteSpace(config.Address)) return;
            if (string.IsNullOrWhiteSpace(config.QQ) || string.IsNullOrWhiteSpace(config.GroupId)) return;
            if (QQ != null) return;
            //add qq bot
            QQ = new MiraiQQBot<T>(config, Hub);
        }

      
    }
}
