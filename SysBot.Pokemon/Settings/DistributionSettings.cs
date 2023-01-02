using PKHeX.Core;
using SysBot.Base;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class DistributionSettings : ISynchronizationSetting
    {
        private const string Distribute = nameof(Distribute);
        private const string Synchronize = nameof(Synchronize);
        public override string ToString() => "派送交换设置Distribution Trade Settings";

        // Distribute

        [Category(Distribute), Description("当启用时，空闲的派送交换机器人将随机从DistributeFolder分发PKM文件\nWhen enabled, idle LinkTrade bots will randomly distribute PKM files from the DistributeFolder.")]
        public bool DistributeWhileIdle { get; set; } = true;

        [Category(Distribute), Description("当启用时，空闲的派送交换机器人将随机从DistributeFolder分发PKM文件\nWhen enabled, the DistributionFolder will yield randomly rather than in the same sequence.")]
        public bool Shuffled { get; set; }

        [Category(Distribute), Description("当设置为None以外的值时，除了匹配昵称外，随机交易还需要这个物种\nWhen set to something other than None, the Random Trades will require this species in addition to the nickname match.")]
        public Species LedySpecies { get; set; } = Species.None;

        [Category(Distribute), Description("当设置为true时，随机的Ledy nickname-swap交易将退出，而不是从交易池中随机交易一个实体\nWhen set to true, Random Ledy nickname-swap trades will quit rather than trade a random entity from the pool.")]
        public bool LedyQuitIfNoMatch { get; set; }

        [Category(Distribute), Description("派送交换连接密码\nDistribution Trade Link Code.")]
        public int TradeCode { get; set; } = 7196;

        [Category(Distribute), Description("派送交换连接密码使用最小值和最大值范围，而不是固定的贸易代码\nDistribution Trade Link Code uses the Min and Max range rather than the fixed trade code.")]
        public bool RandomCode { get; set; }

        [Category(Distribute), Description("对于BDSP，分发机器人将进入一个特定的房间并保持在那里，直到机器人停止\nFor BDSP, the distribution bot will go to a specific room and remain there until the bot is stopped.")]
        public bool RemainInUnionRoomBDSP { get; set; } = true;

        // Synchronize

        [Category(Synchronize), Description("派送交换:使用多个分发机器人——所有机器人将同时确认他们的交易代码。当本地时，当所有的机器人都到达屏障时，机器人将继续。当远程时，必须有其他东西发出信号让机器人继续\nLink Trade: Using multiple distribution bots -- all bots will confirm their trade code at the same time. When Local, the bots will continue when all are at the barrier. When Remote, something else must signal the bots to continue.")]
        public BotSyncOption SynchronizeBots { get; set; } = BotSyncOption.LocalSync;

        [Category(Synchronize), Description("派送交换:使用多个分发机器人——一旦所有机器人都准备好确认交易代码，中心将等待X毫秒，然后释放所有机器人\nLink Trade: Using multiple distribution bots -- once all bots are ready to confirm trade code, the Hub will wait X milliseconds before releasing all bots.")]
        public int SynchronizeDelayBarrier { get; set; }

        [Category(Synchronize), Description("派送交换:使用多个分发机器人——在继续之前，机器人将等待多长时间(秒)同步。Link Trade: Using multiple distribution bots -- how long (seconds) a bot will wait for synchronization before continuing anyways.")]
        public double SynchronizeTimeout { get; set; } = 90;

    }
}