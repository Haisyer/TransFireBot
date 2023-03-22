using PKHeX.Core;
using SysBot.Base;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class DistributionSettings : ISynchronizationSetting
    {
        private const string Distribute = nameof(Distribute);
        private const string Synchronize = nameof(Synchronize);
        public override string ToString() => "派送机器人设置";

        // Distribute

        [Category(Distribute), Description("当启用时，空闲的连接交换机器人将从Distribute文件夹分发PKM文件。")]
        public bool DistributeWhileIdle { get; set; } = true;

        [Category(Distribute), Description("当启用后，Distribution文件夹将随机产生，而不是按照相同的顺序产生。")]
        public bool Shuffled { get; set; }

        [Category(Distribute), Description("当设置为None以外的值时，随机交易除了匹配昵称外，还需要匹配这个种类的宝可梦。")]
        public Species LedySpecies { get; set; } = Species.None;

        [Category(Distribute), Description("当设置为True时，随机的Ledy nickname-swap交易将退出，而不是从交易文件夹中随机交易一个宝可梦。")]
        public bool LedyQuitIfNoMatch { get; set; }

        [Category(Distribute), Description("派送交换连接密码(0-99999999)")]
        public int TradeCode { get; set; } = 7196;

        [Category(Distribute), Description("派送交换连接密码使用最小值和最大值范围，而不是固定的交换密码。")]
        public bool RandomCode { get; set; }

        [Category(Distribute), Description("对于[珍钻]，派送机器人将进入一个特定的房间并保持在那里，直到机器人停止。")]
        public bool RemainInUnionRoomBDSP { get; set; } = true;

        // Synchronize

        [Category(Synchronize), Description("派送交换:使用多个派送机器人 —— 所有机器人将同时确认他们的交换密码。当本地时，当所有的机器人都到达屏障时，机器人将继续。当远程时，必须有其他东西发出信号让机器人继续。")]
        public BotSyncOption SynchronizeBots { get; set; } = BotSyncOption.LocalSync;

        [Category(Synchronize), Description("派送交换:使用多个派送机器人 —— 一旦所有机器人都准备好确认交换密码，中心将等待X毫秒，然后释放所有机器人。")]
        public int SynchronizeDelayBarrier { get; set; }

        [Category(Synchronize), Description("派送交换:使用多个派送机器人 —— 在继续之前，机器人将等待多长时间(秒)后同步。")]
        public double SynchronizeTimeout { get; set; } = 90;

    }
}