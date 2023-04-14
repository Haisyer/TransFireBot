using PKHeX.Core;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon
{
    public class RaidSettings : IBotStateSettings, ICountSettings
    {
        private const string Hosting = nameof(Hosting);
        private const string Counts = nameof(Counts);
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "剑盾团体战机器人设置";

        [Category(Hosting), Description("团体战开始前等待的秒数。取值范围是0 ~ 180秒。")]
        public int TimeToWait { get; set; } = 90;

        [Category(Hosting), Description("团体战的最小连接密码. 将其设置为-1，则表示没有连接密码。")]
        public int MinRaidCode { get; set; } = 8180;

        [Category(Hosting), Description("团体战的最大连接密码. 将其设置为-1，则表示没有连接密码。")]
        public int MaxRaidCode { get; set; } = 8199;

        [Category(FeatureToggle), Description("机器人团体战可选择类型。如果留空，则使用宝可梦自动检测。")]
        public string RaidDescription { get; set; } = string.Empty;

        [Category(FeatureToggle), Description("在每个成员确定宝可梦时，是否发出回应。")]
        public bool EchoPartyReady { get; set; } = false;

        [Category(FeatureToggle), Description("如果设置了，将允许机器人返还你的好友代码。")]
        public string FriendCode { get; set; } = string.Empty;

        [Category(Hosting), Description("每次接受的好友请求数量。")]
        public int NumberFriendsToAdd { get; set; } = 0;

        [Category(Hosting), Description("每次要删除的好友数量。")]
        public int NumberFriendsToDelete { get; set; } = 0;

        [Category(Hosting), Description("在添加/删除好友前要进行的团体战次数。设置值为1，将告诉机器人进行一次团体战，然后开始添加/删除朋友。")]
        public int InitialRaidsToHost { get; set; } = 0;

        [Category(Hosting), Description("添加的好友之间的团体战数量。")]
        public int RaidsBetweenAddFriends { get; set; } = 0;

        [Category(Hosting), Description("删除的好友之间的团体战数量。")]
        public int RaidsBetweenDeleteFriends { get; set; } = 0;

        [Category(Hosting), Description("开始尝试添加好友的数量。")]
        public int RowStartAddingFriends { get; set; } = 1;

        [Category(Hosting), Description("开始尝试删除好友的数量。")]
        public int RowStartDeletingFriends { get; set; } = 1;

        [Category(Hosting), Description("用于管理好友的任天堂Switch配置文件。例如，如果您正在使用第二个配置文件，则将其设置为2。")]
        public int ProfileNumber { get; set; } = 1;

        [Category(FeatureToggle), Description("当启用后，在正常的机器人操作循环中，屏幕将被关闭，以节省电力。")]
        public bool ScreenOff { get; set; } = false;

        /// <summary>
        /// Gets a random trade code based on the range settings.
        /// </summary>
        public int GetRandomRaidCode() => Util.Rand.Next(MinRaidCode, MaxRaidCode + 1);

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
    }
}