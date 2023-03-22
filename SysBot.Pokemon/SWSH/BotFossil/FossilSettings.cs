using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class FossilSettings : IBotStateSettings, ICountSettings
    {
        private const string Fossil = nameof(Fossil);
        private const string Counts = nameof(Counts);
        public override string ToString() => "剑盾化石机器人设置";

        [Category(Fossil), Description("选择化石宝可梦种类")]
        public FossilSpecies Species { get; set; } = FossilSpecies.Dracozolt;

        /// <summary>
        /// Toggle for injecting fossil pieces.
        /// </summary>
        [Category(Fossil), Description("是否切换注入化石碎片")]
        public bool InjectWhenEmpty { get; set; } = false;

        /// <summary>
        /// Toggle for continuing to revive fossils after condition has been met.
        /// </summary>
        [Category(Fossil), Description("当启用后，机器人将在找到合适的匹配后继续工作。")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(Fossil), Description("当启用后，在正常的机器人操作循环中，屏幕将被关闭，以节省电力。")]
        public bool ScreenOff { get; set; } = false;

        private int _completedFossils;

        [Category(Counts), Description("已复活的化石宝可梦。")]
        public int CompletedFossils
        {
            get => _completedFossils;
            set => _completedFossils = value;
        }

        [Category(Counts), Description("当启用后，当要求进行状态检查时，将发出计数。")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedFossils() => Interlocked.Increment(ref _completedFossils);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedFossils != 0)
                yield return $"Completed Fossils: {CompletedFossils}";
        }
    }
}