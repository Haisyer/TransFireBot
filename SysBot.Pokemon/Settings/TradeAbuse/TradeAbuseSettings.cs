using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class TradeAbuseSettings
    {
        private const string Monitoring = nameof(Monitoring);
        public override string ToString() => "交易滥用监控设置";

        [Category(Monitoring), Description("当一个人在小于此设置值（分钟）的时间内再次出现时，将发送通知。")]
        public double TradeCooldown { get; set; }

        [Category(Monitoring), Description("当一个人忽略了交易的冷却时间，Echo信息将包括他们的任天堂账户ID。")]
        public bool EchoNintendoOnlineIDCooldown { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当用户违反交易冷却时间时通知你指定的人。对于Discord用户，使用<@userIDnumber>来提及。")]
        public string CooldownAbuseEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("当一个人以不同的Discord/Twitch账户出现在少于此设置值（分钟）的时间内，将发出通知。")]
        public double TradeAbuseExpiration { get; set; } = 120;

        [Category(Monitoring), Description("当检测到一个人使用多个Discord/Twitch账户时，回音信息将包括他们的任天堂账户ID。")]
        public bool EchoNintendoOnlineIDMulti { get; set; } = true;

        [Category(Monitoring), Description("当检测到一个人向多个游戏内账户发送时，回音信息将包括其任天堂账户ID。")]
        public bool EchoNintendoOnlineIDMultiRecipients { get; set; } = true;

        [Category(Monitoring), Description("当检测到一个人使用多个Discord/Twitch账户时，将采取以下行动。")]
        public TradeAbuseAction TradeAbuseAction { get; set; } = TradeAbuseAction.Quit;

        [Category(Monitoring), Description("当一个人在游戏中因多个账户被封杀时，其在线ID会被添加到BannedIDs中。")]
        public bool BanIDWhenBlockingUser { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当发现一个用户使用多个账户时，通知你指定的人。对于Discord用户，使用<@userIDnumber>来提及。")]
        public string MultiAbuseEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当发现一个用户在游戏中向多个玩家发送信息时，将通知你指定的人。对于Discord用户，使用<@userIDnumber>来提及。")]
        public string MultiRecipientEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("被禁止的在线ID，将触发交易退出或游戏内屏蔽。")]
        public RemoteControlAccessList BannedIDs { get; set; } = new();

        [Category(Monitoring), Description("被禁止的模板ID")]
        public RemoteControlAccessList BanFile { get; set; } = new();

        [Category(Monitoring), Description("当遇到一个被禁止的ID的人，在退出交易之前在游戏中阻止他们。")]
        public bool BlockDetectedBannedUser { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当一个用户与被禁止的ID相匹配时，通知你指定的人。对于Discord用户，使用<@userIDnumber>来提。")]
        public string BannedIDMatchEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("当使用Ledy昵称交换的人被发现滥用时，回音信息将包括他们的任天堂账户ID。")]
        public bool EchoNintendoOnlineIDLedy { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当用户违反Ledy交易规则时，会通知你指定的人。对于Discord用户，使用<@userIDnumber>来提及。")]
        public string LedyAbuseEchoMention { get; set; } = string.Empty;
    }
}
