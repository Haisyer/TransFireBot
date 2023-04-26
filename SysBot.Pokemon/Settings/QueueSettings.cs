using System;
using System.ComponentModel;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon
{
    public class QueueSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string UserBias = nameof(UserBias);
        private const string TimeBias = nameof(TimeBias);
        private const string QueueToggle = nameof(QueueToggle);
        public override string ToString() => "队列加入设置";

        // General

        [Category(FeatureToggle), Description("切换用户是否可以加入队列")]
        public bool CanQueue { get; set; } = true;

        [Category(FeatureToggle), Description("如果队列中已经有该数量的用户，则禁止用户加入队列。")]
        public int MaxQueueCount { get; set; } = 999;

        [Category(FeatureToggle), Description("允许用户在交易时取消排队")]
        public bool CanDequeueIfProcessing { get; set; } = true;

        [Category(FeatureToggle), Description("选择Flex模式处理队列的方式")]
        public FlexYieldMode FlexMode { get; set; } = FlexYieldMode.Weighted;

        [Category(FeatureToggle), Description("确定队列何时被打开和关闭")]
        public QueueOpening QueueToggleMode { get; set; } = QueueOpening.Threshold;

        //[Category(FeatureToggle), Description("是否打开批量文件夹功能")]
        //public bool MutiTrade { get; set; }

        [Category(UserBias), Description("批量最大数目")]
        public int MutiMaxNumber { get; set; } = 3;

        // Queue Toggle

        [Category(QueueToggle), Description("Threshold模式：导致队列打开的用户数。")]
        public int ThresholdUnlock { get; set; } = 0;

        [Category(QueueToggle), Description("Threshold模式：导致队列关闭的用户数。")]
        public int ThresholdLock { get; set; } = 30;

        [Category(QueueToggle), Description("Scheduled模式：在队列锁定前被打开的秒数。")]
        public int IntervalOpenFor { get; set; } = 5 * 60;

        [Category(QueueToggle), Description("Scheduled模式：在队列解锁前被关闭的秒数。")]
        public int IntervalCloseFor { get; set; } = 15 * 60;

        // Flex Users

        [Category(UserBias), Description("根据队列中的用户数量，对交易队列的权重进行倾斜。")]
        public int YieldMultCountTrade { get; set; } = 100;

        [Category(UserBias), Description("根据队列中的用户数量，对种子检索队列的权重进行倾斜。")]
        public int YieldMultCountSeedCheck { get; set; } = 100;

        [Category(UserBias), Description("根据队列中的用户数量，对克隆队列的权重进行倾斜。")]
        public int YieldMultCountClone { get; set; } = 100;

        [Category(UserBias), Description("根据队列中的用户数量，对Dump队列的权重进行倾斜。")]
        public int YieldMultCountDump { get; set; } = 100;

        [Category(UserBias), Description("让第几个用户提前准备")]
        public int AlertNumber { get; set; } = 3;

       



        // Flex Time

        [Category(TimeBias), Description("决定是否应将重量加入或乘以总重量")]
        public FlexBiasMode YieldMultWait { get; set; } = FlexBiasMode.Multiply;

        [Category(TimeBias), Description("检查用户加入交换队列后经过的时间，并相应增加队列的权重。")]
        public int YieldMultWaitTrade { get; set; } = 1;

        [Category(TimeBias), Description("检查用户加入Seed检索队列后经过的时间，并相应增加队列的权重。")]
        public int YieldMultWaitSeedCheck { get; set; } = 1;

        [Category(TimeBias), Description("检查用户加入克隆队列后经过的时间，并相应增加队列的权重。")]
        public int YieldMultWaitClone { get; set; } = 1;

        [Category(TimeBias), Description("检查用户加入Dump队列后经过的时间，并相应增加队列的权重。")]
        public int YieldMultWaitDump { get; set; } = 1;

        [Category(TimeBias), Description("乘以队列中用户的数量，从而估计处理该用户所需的时间。")]
        public float EstimatedDelayFactor { get; set; } = 1.1f;

        private int GetCountBias(PokeTradeType type) => type switch
        {
            PokeTradeType.Seed => YieldMultCountSeedCheck,
            PokeTradeType.Clone => YieldMultCountClone,
            PokeTradeType.Dump => YieldMultCountDump,
            _ => YieldMultCountTrade,
        };

        private int GetTimeBias(PokeTradeType type) => type switch
        {
            PokeTradeType.Seed => YieldMultWaitSeedCheck,
            PokeTradeType.Clone => YieldMultWaitClone,
            PokeTradeType.Dump => YieldMultWaitDump,
            _ => YieldMultWaitTrade,
        };

        /// <summary>
        /// Gets the weight of a <see cref="PokeTradeType"/> based on the count of users in the queue and time users have waited.
        /// </summary>
        /// <param name="count">Count of users for <see cref="type"/></param>
        /// <param name="time">Next-to-be-processed user's time joining the queue</param>
        /// <param name="type">Queue type</param>
        /// <returns>Effective weight for the trade type.</returns>
        public long GetWeight(int count, DateTime time, PokeTradeType type)
        {
            var now = DateTime.Now;
            var seconds = (now - time).Seconds;

            var cb = GetCountBias(type) * count;
            var tb = GetTimeBias(type) * seconds;

            return YieldMultWait switch
            {
                FlexBiasMode.Multiply => cb * tb,
                _ => cb + tb,
            };
        }

        /// <summary>
        /// Estimates the amount of time (minutes) until the user will be processed.
        /// </summary>
        /// <param name="position">Position in the queue</param>
        /// <param name="botct">Amount of bots processing requests</param>
        /// <returns>Estimated time in Minutes</returns>
        public float EstimateDelay(int position, int botct) => (EstimatedDelayFactor * position) / botct;
    }

    public enum FlexBiasMode
    {
        Add,
        Multiply,
    }

    public enum FlexYieldMode
    {
        LessCheatyFirst,
        Weighted,
    }

    public enum QueueOpening
    {
        Manual,
        Threshold,
        Interval,
    }
}