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

        [Category(FeatureToggle), Description("URL to Pokémon Automation's Tera Ban List json (or one matching the required structure).")]
        public string BanListURL { get; set; } = "https://raw.githubusercontent.com/PokemonAutomation/ServerConfigs-PA-SHA/main/PokemonScarletViolet/TeraAutoHost-BanList.json";

        [Category(Hosting), Description("在更新黑名单之前的搜查次数。如果想禁用全局黑名单，请将其设置为-1\nAmount of raids before updating the ban list. If you want the global ban list off, set this to -1.")]
        public int RaidsBetweenUpdate { get; set; } = 3;

        [Category(FeatureToggle), Description("Raid embed title.")]
        public string RaidEmbedTitle { get; set; } = "Tera Raid Notification";

        [Category(FeatureToggle), Description("Raid embed description.")]
        public string[] RaidEmbedDescription { get; set; } = Array.Empty<string>();

        [Category(Hosting), Description("Input the Species to post a Thumbnail in the embeds. Ignored if 0.")]
        public Species RaidSpecies { get; set; } = Species.None;

        [Category(Hosting), Description("If the species does not have an alternate form, leave at 0.")]
        public int RaidSpeciesForm { get; set; } = 0;

        [Category(Hosting), Description("If the species is shiny set to true. False for non-shiny.")]
        public bool RaidSpeciesIsShiny { get; set; } = true;

        [Category(Hosting), Description("钛晶宝可梦的个体\nRaid Species IVs")]
        public string RaidSpeciesIVs { get; set; } = String.Empty;

        [Category(Hosting), Description("钛晶宝可梦的性格\nRaid Species nature")]
        public Nature RaidSpeciesNature { get; set; } = Nature.Random;

        [Category(Hosting), Description("钛晶宝可梦的特性\nRaid Species ability")]
        public Ability RaidSpeciesAbility { get; set; } = Ability.Adaptability;

        [Category(FeatureToggle), Description("如果为真，机器人将使用随机代码进行团体战。\nIf true, the bot will use a random code for the raid.")]
        public bool CodeTheRaid { get; set; } = true;

        [Category(FeatureToggle), Description("如果为真，机器人将在嵌入信息中发布团体战房间码\nIf true, the bot will post the raid code in embed info.")]
        public bool CodeInInfo { get; set; } = false;

        [Category(FeatureToggle), Description("If true, split the code and hide with spolier tag")]
        public bool CodeIfSplitHidden { get; set; } = false;

        [Category(Hosting), Description("Catch limit per player before they get added to the ban list automatically. If set to 0 this setting will be ignored.")]
        public int CatchLimit { get; set; } = 0;

        [Category(Hosting), Description("开始团体战前的最小等待秒数\nMinimum amount of seconds to wait before starting a raid.")]
        public int TimeToWait { get; set; } = 90;

        [Category(Hosting), Description("黑名单玩家NID\nUsers NIDs here are banned raiders.")]
        public RemoteControlAccessList RaiderBanList { get; set; } = new() { AllowIfEmpty = false };

        [Category(FeatureToggle), Description("在日期/时间设置中设置您的切换日期/时间格式。如果“日期”发生变化，日期将自动回退1。\nSet your Switch Date/Time format in the Date/Time settings. The day will automatically rollback by 1 if the Date changes.")]
        public DTFormat DateTimeFormat { get; set; } = DTFormat.MMDDYY;

        [Category(Hosting), Description("向下滚动持续时间(以毫秒为单位)的时间，以便在滚动修正期间访问日期/时间设置。你想让它超过1的日期/时间设置，因为它将点击DUP后向下滚动。(默认值:930毫秒)\nTime to scroll down duration in milliseconds for accessing date/time settings during rollover correction. You want to have it overshoot the Date/Time setting by 1, as it will click DUP after scrolling down. [Default: 930ms]")]
        public int HoldTimeForRollover { get; set; } = 930;

        [Category(FeatureToggle), Description("如果为真，当你在游戏关闭的主屏幕时启动机器人。机器人将只运行翻转例程，因此您可以尝试配置准确的计时。\nIf true, start the bot when you are on the HOME screen with the game closed. The bot will only run the rollover routine so you can try to configure accurate timing.")]
        public bool ConfigureRolloverCorrection { get; set; } = false;

        [Category(FeatureToggle), Description("如果为真，机器人将尝试为突袭嵌入者截屏。如果你经常因为“大小/参数”而崩溃，试着把这个设置为false。\nIf true, the bot will attempt take screenshots for the Raid Embeds. If you experience crashes often about \"Size/Parameter\" try setting this to false.")]
        public bool TakeScreenshot { get; set; } = true;

        [Category(Hosting), Description("Enter Discord channel ID(s) to post raid embeds to. Feature has to be initialized via \"$resv\" after every client restart.")]
        public string RaidEmbedChannelsSV { get; set; } = string.Empty;

        [Category(FeatureToggle), Description("启用后，在正常的bot循环操作时，屏幕将被关闭以节省电力。\nWhen enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }


        private int _completedRaids;

        [Category(Counts), Description("Raids Started")]
        public int CompletedRaids
        {
            get => _completedRaids;
            set => _completedRaids = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
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