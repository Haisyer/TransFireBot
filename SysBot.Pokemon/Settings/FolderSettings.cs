using System.ComponentModel;
using System.IO;

namespace SysBot.Pokemon
{
    public class FolderSettings : IDumper
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Files = nameof(Files);
        public override string ToString() => "文件夹/转储设置";

        [Category(FeatureToggle), Description("启用后，将接收到所有PKM文件(交易结果)并转储到Dump文件夹。")]
        public bool Dump { get; set; }

        [Category(Files), Description("源文件夹（distribute）：选择要派送的PKM文件的地方。")]
        public string DistributeFolder { get; set; } = string.Empty;

        [Category(Files), Description("目的文件夹（dump）：所有接收到的PKM文件被转储到的地方。")]
        public string DumpFolder { get; set; } = string.Empty;

        [Category(Files), Description("dodo批量交换根目录（tradefolder）")]
        public string TradeFolder { get; set; } = string.Empty;

        [Category(Files), Description("qq批量文件根目录（tradesavefile）")]
        public string TradeSaveFile { get; set; } = string.Empty;

        [Category(Files), Description("截图根目录（screenshot）")]
        public string ScreenshotFolder{ get; set; } = string.Empty;

       
        public void CreateDefaults(string path)
        {
            var dump = Path.Combine(path, "dump");
            Directory.CreateDirectory(dump);
            DumpFolder = dump;
            Dump = true;

            var distribute = Path.Combine(path, "distribute");
            Directory.CreateDirectory(distribute);
            DistributeFolder = distribute;

            var tradefolder = Path.Combine(path, "tradefolder");
            Directory.CreateDirectory(tradefolder);
            TradeFolder = tradefolder;

            var tradesavefile = Path.Combine(path, "tradesavefile");
            Directory.CreateDirectory(tradesavefile);
            TradeSaveFile = tradesavefile;

            var screenshot = Path.Combine(path, "screenshot");
            Directory.CreateDirectory(screenshot);
            ScreenshotFolder = screenshot;

        }
    }
}