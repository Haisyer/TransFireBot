using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class TimingSettings
    {
        private const string OpenGame = nameof(OpenGame);
        private const string CloseGame = nameof(CloseGame);
        private const string Raid = nameof(Raid);
        private const string Misc = nameof(Misc);
        public override string ToString() => "额外时间设置Extra Time Settings";

        // Opening the game.
        [Category(OpenGame), Description("启动游戏时等待配置文件加载的额外时间，以毫秒为单位\n time in milliseconds to wait for profiles to load when starting the game.")]
        public int ExtraTimeLoadProfile { get; set; } = 0;

        [Category(OpenGame), Description("检查DLC是否可用的额外等待时间，以毫秒为单位\nExtra time in milliseconds to wait to check if DLC is usable.")]
        public int ExtraTimeCheckDLC { get; set; } = 0;

        [Category(OpenGame), Description("在标题屏幕上点击A之前要等待的额外时间（毫秒)\nExtra time in milliseconds to wait before clicking A in title screen.")]
        public int ExtraTimeLoadGame { get; set; } = 5000;

        [Category(OpenGame), Description("[BDSP]在标题屏幕之后，等待主世界加载的额外时间（毫秒）\n[BDSP] Extra time in milliseconds to wait for the overworld to load after the title screen.")]
        public int ExtraTimeLoadOverworld { get; set; } = 3000;

        // Closing the game.
        [Category(CloseGame), Description("按HOME键最小化游戏后等待的额外时间（毫秒)\nExtra time in milliseconds to wait after pressing HOME to minimize the game.")]
        public int ExtraTimeReturnHome { get; set; } = 0;

        [Category(CloseGame), Description("点击关闭游戏后要等待的额外时间，以毫秒计\nExtra time in milliseconds to wait after clicking to close the game.")]
        public int ExtraTimeCloseGame { get; set; } = 0;

        // Raid-specific timings.
        [Category(Raid), Description("[RaidBot]点击巢穴后，等待Raid加载的额外时间，以毫秒计\n[RaidBot] Extra time in milliseconds to wait for the raid to load after clicking on the den.")]
        public int ExtraTimeLoadRaid { get; set; } = 0;

        [Category(Raid), Description("[RaidBot] 点击 \"邀请他人 \"后，在锁定一个神奇宝贝之前，需要等待额外的时间（毫秒）\n[RaidBot] Extra time in milliseconds to wait after clicking \"Invite Others\" before locking into a Pokémon.")]
        public int ExtraTimeOpenRaid { get; set; } = 0;

        [Category(Raid), Description("[RaidBot]关闭游戏重置突袭前的额外等待时间，以毫秒计\n[RaidBot] Extra time in milliseconds to wait before closing the game to reset the raid.")]
        public int ExtraTimeEndRaid { get; set; } = 0;

        [Category(Raid), Description("[RaidBot]接受朋友后的额外等待时间（毫秒）\n[RaidBot] Extra time in milliseconds to wait after accepting a friend.")]
        public int ExtraTimeAddFriend { get; set; } = 0;

        [Category(Raid), Description("[RaidBot]删除好友后的额外等待时间（毫秒）\n[RaidBot] Extra time in milliseconds to wait after deleting a friend.")]
        public int ExtraTimeDeleteFriend { get; set; } = 0;

        // Miscellaneous settings.
        [Category(Misc), Description("[SWSH/SV] 点击 \"+\"连接到Y-Comm（SWSH）或 \"L \"连接到在线（SV）后的额外等待时间（毫秒）\n[SWSH/SV] Extra time in milliseconds to wait after clicking + to connect to Y-Comm (SWSH) or L to connect online (SV).")]
        public int ExtraTimeConnectOnline { get; set; } = 0;

        [Category(Misc), Description("[BDSP]在离开联机室后，等待主世界加载的额外时间，以毫秒计\n[BDSP] Extra time in milliseconds to wait for the overworld to load after leaving the Union Room.")]
        public int ExtraTimeLeaveUnionRoom { get; set; } = 1000;

        [Category(Misc), Description("[BDSP]在每个交易循环开始时等待Y菜单加载的额外时间，以毫秒计\n[BDSP] Extra time in milliseconds to wait for the Y Menu to load at the start of each trade loop.")]
        public int ExtraTimeOpenYMenu { get; set; } = 500;

        [Category(Misc), Description("[BDSP]在尝试调用交易之前，等待联机室加载的额外时间，以毫秒为单位\n Extra time in milliseconds to wait for the Union Room to load before trying to call for a trade.")]
        public int ExtraTimeJoinUnionRoom { get; set; } = 500;

        [Category(Misc), Description("[SV] 等待宝可站加载的额外时间（毫秒）\n[SV] Extra time in milliseconds to wait for the Poké Portal to load.")]
        public int ExtraTimeLoadPortal { get; set; } = 1000;

        [Category(Misc), Description("找到交易后等待盒子加载的额外时间，以毫秒计\nExtra time in milliseconds to wait for the box to load after finding a trade.")]
        public int ExtraTimeOpenBox { get; set; } = 1000;

        [Category(Misc), Description("交易中打开键盘输入代码后的等待时间\nTime to wait after opening the keyboard for code entry during trades.")]
        public int ExtraTimeOpenCodeEntry { get; set; } = 1000;

        [Category(Misc), Description("在浏览交换菜单或输入链接密码时，每次按键后需要等待的时间\nTime to wait after each keypress when navigating Switch menus or entering Link Code.")]
        public int KeypressTime { get; set; } = 200;

        [Category(Misc), Description("启用此功能可拒绝接收到的系统更新\nEnable this to decline incoming system updates.")]
        public bool AvoidSystemUpdate { get; set; } = false;
    }
}
