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
        public override string ToString() => "交易机器人设置";

        [Category(TradeConfig), Description("等待交换对象的时间（秒）")]
        public int TradeWaitTime { get; set; } = 30;

        [Category(TradeConfig), Description("按A键等待交易处理的最大时间（秒）")]
        public int MaxTradeConfirmTime { get; set; } = 25;

        [Category(TradeCode), Description("最小连接密码(0-99999999)")]
        public int MinTradeCode { get; set; } = 0;

        [Category(TradeCode), Description("最大连接密码(0-99999999)")]
        public int MaxTradeCode { get; set; } = 99999999;

        [Category(Dumping), Description("Dump交易：Dump程序将在单个用户达到最大Dump次数后停止。")]
        public int MaxDumpsPerTrade { get; set; } = 20;

        [Category(Dumping), Description("Dump交易：在交易中等待x秒后，Dump程序将停止。")]
        public int MaxDumpTradeTime { get; set; } = 180;

        [Category(Dumping), Description("如果启用，Dump程序将向用户输出合法性检查信息")]
        public bool DumpTradeLegalityCheck { get; set; } = true;

        [Category(TradeConfig), Description("当启用后，在正常的机器人操作循环中，屏幕将被关闭，以节省电力。")]
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

        [Category(Counts), Description("已完成的魔法交换")]
        public int CompletedSurprise
        {
            get => _completedSurprise;
            set => _completedSurprise = value;
        }

        [Category(Counts), Description("已完成的连接交换（派送）")]
        public int CompletedDistribution
        {
            get => _completedDistribution;
            set => _completedDistribution = value;
        }

        [Category(Counts), Description("已完成的连接交换（特定用户）")]
        public int CompletedTrades
        {
            get => _completedTrades;
            set => _completedTrades = value;
        }

        [Category(Counts), Description("已完成的seed检索交易")]
        public int CompletedSeedChecks
        {
            get => _completedSeedChecks;
            set => _completedSeedChecks = value;
        }

        [Category(Counts), Description("已完成的克隆交易（特定用户）")]
        public int CompletedClones
        {
            get => _completedClones;
            set => _completedClones = value;
        }

        [Category(Counts), Description("已完成的Dump交易（特定用户")]
        public int CompletedDumps
        {
            get => _completedDumps;
            set => _completedDumps = value;
        }

        [Category(Counts), Description("当启用后，当要求进行状态检查时，将发出计数。")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public void AddCompletedTrade() => Interlocked.Increment(ref _completedTrades);
        public void AddCompletedSeedCheck() => Interlocked.Increment(ref _completedSeedChecks);
        public void AddCompletedSurprise() => Interlocked.Increment(ref _completedSurprise);
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
