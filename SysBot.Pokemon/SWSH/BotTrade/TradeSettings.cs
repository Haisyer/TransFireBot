using System.Collections.Generic;
using PKHeX.Core;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class TradeSettings : IBotStateSettings, ICountSettings
    {
        private const string TradeCode = nameof(TradeCode);
        private const string TradeConfig = nameof(TradeConfig);
        private const string Dumping = nameof(Dumping);
        private const string Counts = nameof(Counts);
        public override string ToString() => "交易机器人设置Trade Bot Settings";

        [Category(TradeConfig), Description("等待交换对象的时间,以秒计\nTime to wait for a trade partner in seconds.")]
        public int TradeWaitTime { get; set; } = 30;

        [Category(TradeConfig), Description("按A键等待交易处理的最大时间（秒）\nMax amount of time in seconds pressing A to wait for a trade to process.")]
        public int MaxTradeConfirmTime { get; set; } = 25;

        [Category(TradeCode), Description("最小链接密码(0-99999999)\nMinimum Link Code.")]
        public int MinTradeCode { get; set; } = 0;

        [Category(TradeCode), Description("最大链接密码(0-99999999)\nMaximum Link Code.")]
        public int MaxTradeCode { get; set; } = 99999999;

        [Category(Dumping), Description("Dump交易:倾销程序将在单个用户的最大倾销次数后停止\nDump Trade: Dumping routine will stop after a maximum number of dumps from a single user.")]
        public int MaxDumpsPerTrade { get; set; } = 20;

        [Category(Dumping), Description("Dump贸易。在交易中花费x秒后，倾销程序将停止\nDump Trade: Dumping routine will stop after spending x seconds in trade.")]
        public int MaxDumpTradeTime { get; set; } = 180;

        [Category(Dumping), Description("Dump贸易的合法性检查(F/T)\nDump Trade: Dumping routine will stop after spending x seconds in trade.")]
        public bool DumpTradeLegalityCheck { get; set; } = true;

        [Category(TradeConfig), Description("启用后，在正常的机器人循环操作中，屏幕将被关闭，以节省电力\nWhen enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; } = false;

        /// <summary>
        /// Gets a random trade code based on the range settings.
        /// </summary>
        public int GetRandomTradeCode() => Util.Rand.Next(MinTradeCode, MaxTradeCode + 1);

        private int _completedSurprise;
        private int _completedDistribution;
        private int _completedTrades;
        private int _completedSeedChecks;
        private int _completedClones;
        private int _completedDumps;

        [Category(Counts), Description("已完成的Surprise交易\nCompleted Surprise Trades")]
        public int CompletedSurprise
        {
            get => _completedSurprise;
            set => _completedSurprise = value;
        }

        [Category(Counts), Description("已完成的链接交易（分配）\nCompleted Link Trades (Distribution)")]
        public int CompletedDistribution
        {
            get => _completedDistribution;
            set => _completedDistribution = value;
        }

        [Category(Counts), Description("已完成的链接交易（特定用户）\nCompleted Link Trades (Specific User)")]
        public int CompletedTrades
        {
            get => _completedTrades;
            set => _completedTrades = value;
        }

        [Category(Counts), Description("已完成的种子检查交易\nCompleted Seed Check Trades")]
        public int CompletedSeedChecks
        {
            get => _completedSeedChecks;
            set => _completedSeedChecks = value;
        }

        [Category(Counts), Description("已完成的克隆交易（特定用户）\nCompleted Clone Trades (Specific User)")]
        public int CompletedClones
        {
            get => _completedClones;
            set => _completedClones = value;
        }

        [Category(Counts), Description("已完成的倾销交易（特定用户）\nCompleted Dump Trades (Specific User)")]
        public int CompletedDumps
        {
            get => _completedDumps;
            set => _completedDumps = value;
        }

        [Category(Counts), Description("启用后，当要求进行状态检查时，将发出计数\nWhen enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public void AddCompletedTrade() => Interlocked.Increment(ref _completedTrades);
        public void AddCompletedSeedCheck() => Interlocked.Increment(ref _completedSeedChecks);
        public void AddCompletedSurprise() =>Interlocked.Increment(ref _completedSurprise);
        public void AddCompletedDistribution() => Interlocked.Increment(ref _completedDistribution);
        public void AddCompletedDumps() => Interlocked.Increment(ref _completedDumps);
        public void AddCompletedClones() => Interlocked.Increment(ref _completedClones);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedSeedChecks != 0)
                yield return $"Seed Check Trades: {CompletedSeedChecks}";
            if (CompletedClones != 0)
                yield return $"Clone Trades: {CompletedClones}";
            if (CompletedDumps != 0)
                yield return $"Dump Trades: {CompletedDumps}";
            if (CompletedTrades != 0)
                yield return $"Link Trades: {CompletedTrades}";
            if (CompletedDistribution != 0)
                yield return $"Distribution Trades: {CompletedDistribution}";
            if (CompletedSurprise != 0)
                yield return $"Surprise Trades: {CompletedSurprise}";
        }
    }
}
