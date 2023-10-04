using System;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon
{
    public class DodoSettings
    {
        private const string Startup = nameof(Startup);

        public override string ToString() => "Dodo整合设置";

        // Startup

        [Category(Startup), Description("接口地址")]
        public string BaseApi { get; set; } = "https://botopen.imdodo.com";

        [Category(Startup), Description("机器人唯一标识")]
        public string ClientId { get; set; } = string.Empty;

        [Category(Startup), Description("机器人鉴权Token")]
        public string Token { get; set; } = string.Empty;

        [Category(Startup), Description("机器人响应频道id")]
        public string ChannelId { get; set; } = string.Empty;

        [Category(Startup), Description("是否发送截图到dodo服务器")]
        public bool DodoScreenshot { get; set; } = false;

        [Category(Startup), Description("Dodo上传文件授权链接\nBot+空格+识别码+.+Token\n实例：Bot 69804372.Njk4MDQzNzI.77-9OW_vv70.qvJQfqTiyAXPJlZx1THOL8hp2H3MjISyFpficc6OOOM")]
        public string DodoUploadFileUrl { get; set; } = string.Empty;

        [Category(Startup), Description("可以插队的身份组")]
        public string VipRole { get; set; } = "1111111";

        [Category(Startup), Description("可以批量的身份组")]
        public string BatchRole { get; set; } = "1111111";

        [Category(Startup), Description("是否撤回交换消息")]
        public bool WithdrawTradeMessage { get; set; } = false;

        [Category(Startup), Description("是否开启卡片消息")]
        public bool CardTradeMessage { get; set; } = true;
    }
}