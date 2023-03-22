using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class EggSettings : IBotStateSettings, ICountSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Counts = nameof(Counts);
        public override string ToString() => "蛋机器人设置";

        [Category(FeatureToggle), Description("当启用后，机器人将在找到合适的匹配后继续工作。")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(FeatureToggle), Description("当启用后，在正常的机器人操作循环中，屏幕将被关闭，以节省电力。")]
        public bool ScreenOff { get; set; } = false;

        private int _completedEggs;

        [Category(Counts), Description("已取回的蛋")]
        public int CompletedEggs
        {
            get => _completedEggs;
            set => _completedEggs = value;
        }

        [Category(Counts), Description("当启用后，当要求进行状态检查时，将发出计数。")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedEggs() => Interlocked.Increment(ref _completedEggs);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedEggs != 0)
                yield return $"Eggs Received: {CompletedEggs}";
        }
    }
}
