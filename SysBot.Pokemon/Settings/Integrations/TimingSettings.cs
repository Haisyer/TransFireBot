using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class TimingSettings
    {
        private const string OpenGame = nameof(OpenGame);
        private const string CloseGame = nameof(CloseGame);
        private const string Raid = nameof(Raid);
        private const string Misc = nameof(Misc);
        public override string ToString() => "额外时间设置";

        // Opening the game.
        [Category(OpenGame), Description("启动游戏时等待配置文件加载的额外时间（毫秒)")]
        public int ExtraTimeLoadProfile { get; set; } = 0;

        [Category(OpenGame), Description("检查DLC是否可用的额外等待时间（毫秒)")]
        public int ExtraTimeCheckDLC { get; set; } = 0;

        [Category(OpenGame), Description("在标题屏幕上点击A之前要等待的额外时间（毫秒)")]
        public int ExtraTimeLoadGame { get; set; } = 5000;

        [Category(OpenGame), Description("[珍钻]在标题屏幕之后，等待主世界加载的额外时间（毫秒)")]
        public int ExtraTimeLoadOverworld { get; set; } = 3000;

        // Closing the game.
        [Category(CloseGame), Description("按HOME键最小化游戏后等待的额外时间（毫秒)")]
        public int ExtraTimeReturnHome { get; set; } = 0;

        [Category(CloseGame), Description("点击关闭游戏后要等待的额外时间（毫秒)")]
        public int ExtraTimeCloseGame { get; set; } = 0;

        // Raid-specific timings.
        [Category(Raid), Description("[团体战机器人]点击巢穴后，等待团体战加载的额外时间（毫秒)")]
        public int ExtraTimeLoadRaid { get; set; } = 0;

        [Category(Raid), Description("[团体战机器人] 点击 \"邀请其他人 \"后，在确定一个宝可梦之前，需要等待额外的时间（毫秒)")]
        public int ExtraTimeOpenRaid { get; set; } = 0;

        [Category(Raid), Description("[团体战机器人]关闭游戏重置团体战前的额外等待时间（毫秒)")]
        public int ExtraTimeEndRaid { get; set; } = 0;

        [Category(Raid), Description("[团体战机器人]接受朋友后的额外等待时间（毫秒)")]
        public int ExtraTimeAddFriend { get; set; } = 0;

        [Category(Raid), Description("[团体战机器人]删除好友后的额外等待时间（毫秒)")]
        public int ExtraTimeDeleteFriend { get; set; } = 0;

        // Miscellaneous settings.
        [Category(Misc), Description("[剑盾/朱紫] 点击 \"+\"连接到剑盾通讯界面或点击 \"L \"进行朱紫在线联网后的额外等待时间（毫秒)")]
        public int ExtraTimeConnectOnline { get; set; } = 10000;

        [Category(Misc), Description("连接失败后尝试重新连接到套接字连接的次数. 将其设置为-1则进行无限次地尝试。")]
        public int ReconnectAttempts { get; set; } = 30;

        [Category(Misc), Description("在尝试重新连接之间等待的额外时间（毫秒)")]
        public int ExtraReconnectDelay { get; set; }
        [Category(Misc), Description("[珍钻]在离开联机室后，等待主世界加载的额外时间（毫秒)")]
        public int ExtraTimeLeaveUnionRoom { get; set; } = 1000;

        [Category(Misc), Description("[珍钻]在每个交易循环开始时等待Y菜单加载的额外时间（毫秒)")]
        public int ExtraTimeOpenYMenu { get; set; } = 500;

        [Category(Misc), Description("[珍钻]在尝试调用交易之前，等待联机室加载的额外时间（毫秒)")]
        public int ExtraTimeJoinUnionRoom { get; set; } = 500;

        [Category(Misc), Description("[朱紫] 等待宝可入口站加载的额外时间（毫秒)")]
        public int ExtraTimeLoadPortal { get; set; } = 3000;

        [Category(Misc), Description("找到交易后等待盒子加载的额外时间（毫秒)")]
        public int ExtraTimeOpenBox { get; set; } = 1000;

        [Category(Misc), Description("交易中打开键盘输入密码的等待时间（毫秒)")]
        public int ExtraTimeOpenCodeEntry { get; set; } = 1000;

        [Category(Misc), Description("在浏览交换菜单或输入连接密码时，每次按键后需要等待的时间（毫秒)")]
        public int KeypressTime { get; set; } = 200;

        [Category(Misc), Description("启用此功能可拒绝接收到的系统更新。")]
        public bool AvoidSystemUpdate { get; set; } = false;
    }
}
