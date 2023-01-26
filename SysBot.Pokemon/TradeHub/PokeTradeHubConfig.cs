using System.ComponentModel;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon
{
    public sealed class PokeTradeHubConfig : BaseConfig
    {
        private const string BotTrade = nameof(BotTrade);
        private const string BotEncounter = nameof(BotEncounter);
        private const string Integration = nameof(Integration);

        [Browsable(false)]
        public override bool Shuffled => Distribution.Shuffled;

        [Category(FeatureToggle), Description("在这里设置您的交换机控制台语言，以便SWSH机器人正常工作。所有的控制台都应该使用相同的语言。")]
        public ConsoleLanguageParameter ConsoleLanguage { get; set; }

        [Category(Operation)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QueueSettings Queues { get; set; } = new();

        [Category(Operation), Description("增加额外的时间。\nAdd extra time for slower Switches.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TimingSettings Timings { get; set; } = new();

        // Trade Bots

        [Category(BotTrade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TradeSettings Trade { get; set; } = new();

        [Category(BotTrade), Description("闲置分配交易的设置\nSettings for idle distribution trades.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DistributionSettings Distribution { get; set; } = new();

        [Category(BotTrade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public SeedCheckSettings SeedCheckSWSH { get; set; } = new();

        [Category(BotTrade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TradeAbuseSettings TradeAbuse { get; set; } = new();

        // Encounter Bots - For finding or hosting Pokémon in-game.

        [Category(BotEncounter)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public EncounterSettings EncounterSWSH { get; set; } = new();

        [Category(BotEncounter)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public RaidSettings RaidSWSH { get; set; } = new();

        [Category(BotEncounter)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public RaidSettingsSV RaidSV { get; set; } = new();
        [Category(BotEncounter)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public FossilSettings FossilSWSH { get; set; } = new();

        [Category(BotEncounter)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public EggSettings Egg { get; set; } = new();

        [Category(BotEncounter), Description("EggBot, FossilBot和EncounterBot的停止条件。\nStop conditions for EggBot, FossilBot, and EncounterBot.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public StopConditionSettings StopConditions { get; set; } = new();

       

        // Integration


        [Category(Integration), Description("配置流资源的生成。\nConfigure generation of assets for streaming.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public StreamSettings Stream { get; set; } = new();

        [Category(Integration), Description("允许喜欢的用户以比不喜欢的用户更有利的位置加入队列。\nAllows favored users to join the queue with a more favorable position than unfavored users.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public FavoredPrioritySettings Favoritism { get; set; } = new();

        [Category(Integration)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QQSettings QQ { get; set; } = new();

        [Category(Integration)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public BilibiliSettings Bilibili { get; set; } = new();

        [Category(Integration)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DodoSettings Dodo { get; set; } = new();
    }
}