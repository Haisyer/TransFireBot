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

        [Category(Startup), Description("Mirai Bot地址，默认端口为8080，请与Mirai中的端口保持一致")]
        public string Address { get; set; } = "localhost:8080";

        [Category(Startup), Description("Mirai Bot验证密钥，默认密钥为1234567890 请与Mirai中的密钥保持一致")]
        public string VerifyKey { get; set; } = "1234567890";

        [Category(Startup), Description("你的机器人的QQ号")]
        public string QQ { get; set; } = string.Empty;

        [Category(Startup), Description("发送消息的QQ群号")]
        public string GroupId { get; set; } = string.Empty;

        [Category(Startup), Description("机器人回复的触发词1")]
        public string AliveMsgOne { get; set; } = "关键词1"; 

        [Category(Startup), Description("机器人回复消息1")]
        public string ReplyMsgOne { get; set; } = "回复1";

        [Category(Startup), Description("机器人回复的触发词2")]
        public string AliveMsgTwo { get; set; } = "关键词2";

        [Category(Startup), Description("机器人回复消息2")]
        public string ReplyMsgTwo { get; set; } = "回复2";

        [Category(Startup), Description("机器人回复的触发词3")]
        public string AliveMsgThree { get; set; } = "关键词3";

        [Category(Startup), Description("机器人回复消息3")]
        public string ReplyMsgThree { get; set; } = "回复3";

        [Category(Operation), Description("屏障释放时发送的消息")]
        public string MessageStart { get; set; } = string.Empty;
    }
}