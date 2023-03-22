using PKHeX.Core;
using System.ComponentModel;
using SysBot.Base;
using System.Collections.Generic;
using System.Threading;
using System;

namespace SysBot.Pokemon
{
    public class RaidSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string Hosting = nameof(Hosting);
        private const string Counts = nameof(Counts);
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "朱紫团体战机器人设置";

        [Category(FeatureToggle), Description("将URL转换成宝可梦自动化的太晶禁用列表json格式（或符合所需结构的格式）。")]
        public string BanListURL { get; set; } = "https://raw.githubusercontent.com/PokemonAutomation/ServerConfigs-PA-SHA/main/PokemonScarletViolet/TeraAutoHost-BanList.json";

        [Category(Hosting), Description("在更新黑名单之前的搜查次数。如果想禁用全局黑名单，请将其设置为-1。")]
        public int RaidsBetweenUpdate { get; set; } = 3;

        [Category(FeatureToggle), Description("团体战嵌入标题")]
        public string RaidEmbedTitle { get; set; } = "太晶团体战公告";

        [Category(FeatureToggle), Description("团体战嵌入说明")]
        public string[] RaidEmbedDescription { get; set; } = Array.Empty<string>();

        [Category(Hosting), Description("输入宝可梦种类，在嵌入中发布一个缩略图。如果是0则忽略。")]
        public Species RaidSpecies { get; set; } = Species.None;

        [Category(Hosting), Description("如果该宝可梦种类没有替代形式，则保留为0。")]
        public int RaidSpeciesForm { get; set; } = 0;

        [Category(Hosting), Description("设置为True，宝可梦为闪光。设置为False，宝可梦为非闪。")]
        public bool RaidSpeciesIsShiny { get; set; } = true;

        [Category(Hosting), Description("太晶宝可梦的个体")]
        public string RaidSpeciesIVs { get; set; } = String.Empty;

        [Category(Hosting), Description("太晶宝可梦的性格")]
        public Nature RaidSpeciesNature { get; set; } = Nature.Random;

        [Category(Hosting), Description("太晶宝可梦的特性")]
        public Ability RaidSpeciesAbility { get; set; } = Ability.Adaptability;

        [Category(FeatureToggle), Description("如果为True，机器人将使用随机代码进行团体战。")]
        public bool CodeTheRaid { get; set; } = true;

        [Category(FeatureToggle), Description("如果为True，机器人将在嵌入信息中发布团体战房间码。")]
        public bool CodeInInfo { get; set; } = false;

        [Category(FeatureToggle), Description("如果为True，将拆分团体战房间码，用spolier标签隐藏。")]
        public bool CodeIfSplitHidden { get; set; } = false;

        [Category(Hosting), Description("在他们被自动添加到禁令名单之前，每个玩家的捕捉限制。如果设置为0，该设置将被忽略。")]
        public int CatchLimit { get; set; } = 0;

        [Category(Hosting), Description("开始团体战前的最小等待秒数")]
        public int TimeToWait { get; set; } = 90;

        [Category(Hosting), Description("黑名单玩家NID")]
        public RemoteControlAccessList RaiderBanList { get; set; } = new() { AllowIfEmpty = false };

        [Category(FeatureToggle), Description("在日期/时间设置中设置您的切换日期/时间格式。如果“日期”发生变化，日期将自动回退1")]
        public DTFormat DateTimeFormat { get; set; } = DTFormat.MMDDYY;

        [Category(Hosting), Description("向下滚动持续时间(以毫秒为单位)的时间，以便在滚动修正期间访问日期/时间设置。你想让它超过1的日期/时间设置，因为它将点击DUP后向下滚动。(默认值:930毫秒)")]
        public int HoldTimeForRollover { get; set; } = 930;

        [Category(FeatureToggle), Description("如果为True，当你在游戏关闭的主屏幕时启动机器人。机器人将只运行翻转例程，因此您可以尝试配置准确的计时。")]
        public bool ConfigureRolloverCorrection { get; set; } = false;

        [Category(FeatureToggle), Description("如果为True，机器人将尝试为团体战嵌入截屏。如果你经常因为“大小/参数”而崩溃，试着把这个设置为False。")]
        public bool TakeScreenshot { get; set; } = true;

        [Category(Hosting), Description("输入Discord channel ID(s)以发布团体战嵌入。每个客户端重启后，特性必须通过\"$resv\"进行初始化。")]
        public string RaidEmbedChannelsSV { get; set; } = string.Empty;

        [Category(FeatureToggle), Description("当启用后，在正常的机器人操作循环中，屏幕将被关闭，以节省电力。")]
        public bool ScreenOff { get; set; }


        private int _completedRaids;

        [Category(Counts), Description("已完成的团体战")]
        public int CompletedRaids
        {
            get => _completedRaids;
            set => _completedRaids = value;
        }

        [Category(Counts), Description("当启用后，当要求进行状态检查时，将发出计数。")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedRaids() => Interlocked.Increment(ref _completedRaids);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedRaids != 0)
                yield return $"Started Raids: {CompletedRaids}";
        }

        public enum DTFormat
        { 
            MMDDYY,
            DDMMYY,
            YYMMDD,
        }
    }
}