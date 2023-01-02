using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class TradeAbuseSettings
    {
        private const string Monitoring = nameof(Monitoring);
        public override string ToString() => "交易滥用监控设置Trade Abuse Monitoring Settings";

        [Category(Monitoring), Description("当一个人在小于此设置值（分钟）的时间内再次出现时，将发送通知\nWhen a person appears again in less than this setting's value (minutes), a notification will be sent.")]
        public double TradeCooldown { get; set; }

        [Category(Monitoring), Description("当一个人忽略了交易的冷却时间，Echo信息将包括他们的任天堂账户ID\nWhen a person ignores a trade cooldown, the echo message will include their Nintendo Account ID.")]
        public bool EchoNintendoOnlineIDCooldown { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当用户违反交易冷却时间时通知你指定的人。对于Discord，使用<@userIDnumber>来提及\nIf not empty, the provided string will be appended to Echo alerts to notify whomever you specify when a user violates trade cooldown. For Discord, use <@userIDnumber> to mention.")]
        public string CooldownAbuseEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("当一个人以不同的Discord/Twitch账户出现在少于此设置值（分钟）的时间内，将发出通知\nWhen a person appears with a different Discord/Twitch account in less than this setting's value (minutes), a notification will be sent.")]
        public double TradeAbuseExpiration { get; set; } = 120;

        [Category(Monitoring), Description("当检测到一个人使用多个Discord/Twitch账户时，回音信息将包括他们的任天堂账户ID\na person using multiple Discord/Twitch accounts is detected, the echo message will include their Nintendo Account ID.")]
        public bool EchoNintendoOnlineIDMulti { get; set; } = true;

        [Category(Monitoring), Description("当检测到一个人向多个游戏内账户发送时，回音信息将包括其任天堂账户ID\nWhen a person sending to multiple in-game accounts is detected, the echo message will include their Nintendo Account ID.")]
        public bool EchoNintendoOnlineIDMultiRecipients { get; set; } = true;

        [Category(Monitoring), Description("当检测到一个人使用多个Discord/Twitch账户时，将采取以下行动\nWhen a person using multiple Discord/Twitch accounts is detected, this action is taken.")]
        public TradeAbuseAction TradeAbuseAction { get; set; } = TradeAbuseAction.Quit;

        [Category(Monitoring), Description("当一个人在游戏中因多个账户被封杀时，其在线ID会被添加到BannedIDs中\nWhen a person is blocked in-game for multiple accounts, their online ID is added to BannedIDs.")]
        public bool BanIDWhenBlockingUser { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当发现一个用户使用多个账户时，通知你指定的人。对于Discord，使用<@userIDnumber>来提及\nIf not empty, the provided string will be appended to Echo alerts to notify whomever you specify when a user is found using multiple accounts. For Discord, use <@userIDnumber> to mention.")]
        public string MultiAbuseEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当发现一个用户在游戏中向多个玩家发送信息时，将通知你指定的人。对于Discord，使用<@userIDnumber>来提及\nIf not empty, the provided string will be appended to Echo alerts to notify whomever you specify when a user is found sending to multiple players in-game. For Discord, use <@userIDnumber> to mention.")]
        public string MultiRecipientEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("被禁止的在线ID，将触发交易退出或游戏内屏蔽\nBanned online IDs that will trigger trade exit or in-game block.")]
        public RemoteControlAccessList BannedIDs { get; set; } = new();

        [Category(Monitoring), Description("当遇到一个被禁止的ID的人，在退出交易之前在游戏中阻止他们\nWhen a person is encountered with a banned ID, block them in-game before quitting the trade.")]
        public bool BlockDetectedBannedUser { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当一个用户与被禁止的ID相匹配时，通知你指定的人。对于Discord，使用<@userIDnumber>来提\nIf not empty, the provided string will be appended to Echo alerts to notify whomever you specify when a user matches a banned ID. For Discord, use <@userIDnumber> to mention.")]
        public string BannedIDMatchEchoMention { get; set; } = string.Empty;

        [Category(Monitoring), Description("当使用Ledy昵称交换的人被发现滥用时，回音信息将包括他们的任天堂账户ID\nWhen a person using Ledy nickname swaps is detected of abuse, the echo message will include their Nintendo Account ID.")]
        public bool EchoNintendoOnlineIDLedy { get; set; } = true;

        [Category(Monitoring), Description("如果不是空的，提供的字符串将被附加到Echo警报中，当用户违反Ledy交易规则时，会通知你指定的人。对于Discord，使用<@userIDnumber>来提及\nIf not empty, the provided string will be appended to Echo alerts to notify whomever you specify when a user violates Ledy trade rules. For Discord, use <@userIDnumber> to mention.")]
        public string LedyAbuseEchoMention { get; set; } = string.Empty;
    }
}
