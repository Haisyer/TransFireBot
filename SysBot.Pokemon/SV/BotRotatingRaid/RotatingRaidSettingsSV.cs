using PKHeX.Core;
using System;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon
{
    public class RotatingRaidSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string Hosting = nameof(Hosting);
        private const string Counts = nameof(Counts);
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "可变朱紫团体战机器人设置";

        [Category(FeatureToggle), Description("将URL转换成宝可梦自动化的太晶禁用列表json格式（或符合所需结构的格式）。")]
        public string BanListURL { get; set; } = "";

        [Category(FeatureToggle), Description("将URL转换成宝可梦自动化的太晶联网禁用列表json格式（或符合所需结构的格式）。")]
        public string GlobalBanListURL { get; set; } = "";

        [Category(Hosting), Description("在更新黑名单之前的搜查次数。如果想禁用全局黑名单，请将其设置为-1")]
        public int RaidsBetweenUpdate { get; set; } = 3;

        [Category(Hosting), Description("启用后，机器人将尝试从机器人启动时的“raidsv.txt”文件自动生成 Raid 参数。")]
        public bool GenerateParametersFromFile { get; set; } = true;

        [Category(Hosting), Description("RotatingRaid Preset Filters"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RotatingRaidPresetFiltersCategory PresetFilters { get; set; } = new();

        [Category(Hosting), Description("Raid embed parameters.")]
        public List<RotatingRaidParameters> RaidEmbedParameters { get; set; } = new();

        [Category(Hosting), Description("输入机器人自动停止之前要托管的袭击总数。默认值为 0 以忽略此设置。")]
        public int TotalRaidsToHost { get; set; } = 0;

        [Category(Hosting), Description("在他们被自动添加到禁令名单之前，每个玩家的捕捉限制。如果设置为0，该设置将被忽略。")]
        public int CatchLimit { get; set; } = 0;

        [Category(Hosting), Description("开始团体战前的最小等待秒数")]
        public int TimeToWait { get; set; } = 90;

        [Category(FeatureToggle), Description("启用后，嵌入将在“TimeToWait”中倒计时秒数，直到开始突袭。")]
        public bool IncludeCountdown { get; set; } = false;

        [Category(Hosting), Description("Lobby Options"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LobbyFiltersCategory LobbyOptions { get; set; } = new();

        [Category(FeatureToggle), Description("如果为True，机器人将尝试为团体战嵌入截屏。如果你经常因为“大小/参数”而崩溃，试着把这个设置为False。")]
        public bool TakeScreenshot { get; set; } = false;

        [Category(FeatureToggle), Description("启用后，机器人将从 Discord 嵌入中隐藏 raid 代码。")]
        public bool HideRaidCode { get; set; } = false;

        [Category(Hosting), Description("被禁止的用户NID")]
        public RemoteControlAccessList RaiderBanList { get; set; } = new() { AllowIfEmpty = false };

        [Category(Hosting), Description("启用后，机器人会把当天的SEED注入到明天，以确保凌晨不会刷新SEED")]
        public bool KeepDaySeed { get; set; } = false;

        [Category(FeatureToggle), Description("在日期/时间设置中设置您的切换日期/时间格式。如果“日期”发生变化，日期将自动回退1")]
        public DTFormat DateTimeFormat { get; set; } = DTFormat.MMDDYY;

        [Category(Hosting), Description("启用后，机器人将使用overshoot方式来应用翻转校正，否则将使用 DDOWN 点击。")]
        public bool UseOvershoot { get; set; } = false;

        [Category(Hosting), Description("在翻转校正期间点击 DDOWN 来访问日期/时间设置的次数。 [默认值：39 次点击]")]
        public int DDOWNClicks { get; set; } = 39;

        [Category(Hosting), Description("向下滚动持续时间(以毫秒为单位)的时间，以便在滚动修正期间访问日期/时间设置。你想让它超过1的日期/时间设置，因为它将点击DUP后向下滚动。(默认值:930毫秒)")]
        public int HoldTimeForRollover { get; set; } = 900;

        [Category(Hosting), Description("如果为True，当你在游戏关闭的主屏幕时启动机器人。机器人将只运行翻转例程，因此您可以尝试配置准确的计时。")]
        public bool ConfigureRolloverCorrection { get; set; } = false;

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

        public class RotatingRaidParameters
        {
            public override string ToString() => $"{Title}";
            public bool ActiveInRotation { get; set; } = true;
            public TeraCrystalType CrystalType { get; set; } = TeraCrystalType.Base;
            public string[] Description { get; set; } = Array.Empty<string>();
            public bool IsCoded { get; set; } = true;
            public bool IsSet { get; set; } = false;
            public bool IsShiny { get; set; } = true;
            public Species Species { get; set; } = Species.None;
            public int SpeciesForm { get; set; } = 0;
            public string[] PartyPK { get; set; } = Array.Empty<string>();
            public bool SpriteAlternateArt { get; set; } = false;
            public string Seed { get; set; } = "0";
            public string Title { get; set; } = string.Empty;
        }

        [Category(Hosting), TypeConverter(typeof(CategoryConverter<RotatingRaidPresetFiltersCategory>))]
        public class RotatingRaidPresetFiltersCategory
        {
            public override string ToString() => "Preset filters.";

            [Category(Hosting), Description("如果为 true，机器人将尝试根据“preset.txt”文件自动生成 Raid Embed")]
            public bool UsePresetFile { get; set; } = true;

            [Category(Hosting), Description("如果为 true，机器人将使用预设的第一行作为标题。")]
            public bool TitleFromPreset { get; set; } = true;

            [Category(Hosting), Description("如果为 true，机器人将用新标题覆盖任何设置的标题。")]
            public bool ForceTitle { get; set; } = true;

            [Category(Hosting), Description("如果为 true，机器人将用新的描述覆盖任何设置的描述。")]
            public bool ForceDescription { get; set; } = true;

            [Category(Hosting), Description("如果为 true，机器人会将技能附加到预设的描述中。")]
            public bool IncludeMoves { get; set; } = true;

            [Category(Hosting), Description("如果为 true，机器人会将掉落奖励附加到预设描述中。")]
            public bool IncludeRewards { get; set; } = true;
        }

        [Category(Hosting), TypeConverter(typeof(CategoryConverter<LobbyFiltersCategory>))]
        public class LobbyFiltersCategory
        {
            public override string ToString() => "Lobby Filters";

            [Category(Hosting), Description("OpenLobby - Opens the Lobby after x Empty Lobbies\nSkipRaid - Moves on after x losses/empty Lobbies\nContinue - Continues hosting the raid")]
            public LobbyMethodOptions LobbyMethodOptions { get; set; } = LobbyMethodOptions.OpenLobby;

            [Category(Hosting), Description("Empty raid limit per parameter before the bot hosts an uncoded raid. Default is 3 raids.")]
            public int EmptyRaidLimit { get; set; } = 3;

            [Category(Hosting), Description("Empty/Lost raid limit per parameter before the bot moves on to the next one. Default is 3 raids.")]
            public int SkipRaidLimit { get; set; } = 3;

        }

        public class CategoryConverter<T> : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(T));

            public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
        }
    }    
}