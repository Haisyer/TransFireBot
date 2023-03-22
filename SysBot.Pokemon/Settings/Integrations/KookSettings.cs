using System;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon
{
    public class KookSettings
    {
        private const string Startup = nameof(Startup);

        public override string ToString() => "Kook整合设置";

        // Startup
        [Category(Startup), Description("机器人鉴权Token")]
        public string Token { get; set; } = string.Empty;
    }
}