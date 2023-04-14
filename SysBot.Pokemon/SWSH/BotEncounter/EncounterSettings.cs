using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class EncounterSettings : IBotStateSettings, ICountSettings
    {
        private const string Counts = nameof(Counts);
        private const string Encounter = nameof(Encounter);
        private const string Settings = nameof(Settings);
        public override string ToString() => "剑盾遭遇机器人设置";

        [Category(Encounter), Description("Line和Reset机器人遇到宝可梦时使用的方法。")]
        public EncounterMode EncounteringType { get; set; } = EncounterMode.VerticalLine;

        [Category(Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public FossilSettings Fossil { get; set; } = new();

        [Category(Encounter), Description("当启用后，机器人将在找到合适的匹配后继续工作。")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(Encounter), Description("当启用后，在正常的机器人操作循环中，屏幕将被关闭，以节省电力。")]
        public bool ScreenOff { get; set; } = false;

        private int _completedWild;
        private int _completedLegend;
        private int _completedEggs;
        private int _completedFossils;

        [Category(Counts), Description("遭遇的野生宝可梦")]
        public int CompletedEncounters
        {
            get => _completedWild;
            set => _completedWild = value;
        }

        [Category(Counts), Description("遭遇的传说宝可梦")]
        public int CompletedLegends
        {
            get => _completedLegend;
            set => _completedLegend = value;
        }

        [Category(Counts), Description("取蛋")]
        public int CompletedEggs
        {
            get => _completedEggs;
            set => _completedEggs = value;
        }


        [Category(Counts), Description("“复活化石宝可梦")]
        public int CompletedFossils
        {
            get => _completedFossils;
            set => _completedFossils = value;
        }

        [Category(Counts), Description("当启用后，当要求进行状态检查时，将发出计数。")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedEncounters() => Interlocked.Increment(ref _completedWild);
        public int AddCompletedLegends() => Interlocked.Increment(ref _completedLegend);
        public int AddCompletedEggs() => Interlocked.Increment(ref _completedEggs);
        public int AddCompletedFossils() => Interlocked.Increment(ref _completedFossils);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedEncounters != 0)
                yield return $"Wild Encounters: {CompletedEncounters}";
            if (CompletedLegends != 0)
                yield return $"Legendary Encounters: {CompletedLegends}";
            if (CompletedEggs != 0)
                yield return $"Eggs Received: {CompletedEggs}";
            if (CompletedFossils != 0)
                yield return $"Completed Fossils: {CompletedFossils}";
        }
    }
}