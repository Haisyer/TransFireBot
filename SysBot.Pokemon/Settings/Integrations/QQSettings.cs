using System;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon
{
    public class QQSettings
    {
        private const string Startup = nameof(Startup);
        private const string Operation = nameof(Operation);
        private const string Messages = nameof(Messages);
        public override string ToString() => "QQ整合设置";

        // Startup

        [Category(Startup), Description("Mirai Bot地址")]
        public string Address { get; set; } = string.Empty;

        [Category(Startup), Description("Mirai Bot验证密钥")]
        public string VerifyKey { get; set; } = string.Empty;

        [Category(Startup), Description("你的机器人的QQ号")]
        public string QQ { get; set; } = string.Empty;

        [Category(Startup), Description("发送消息的QQ群号")]
        public string GroupId { get; set; } = string.Empty;

        [Category(Startup), Description("机器人打招呼消息")]
        public string AliveMsg { get; set; } = "hello";

        [Category(Operation), Description("屏障释放时发送的消息")]
        public string MessageStart { get; set; } = string.Empty;
    }
}