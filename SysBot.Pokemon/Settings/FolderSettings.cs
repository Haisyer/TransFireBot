using System.ComponentModel;
using System.IO;

namespace SysBot.Pokemon
{
    public class FolderSettings : IDumper
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Files = nameof(Files);
        public override string ToString() => "文件夹/转储设置Folder / Dumping Settings";

        [Category(FeatureToggle), Description("启用后，将接收到的所有PKM文件(交易结果)转储到DumpFolder\nWhen enabled, dumps any received PKM files (trade results) to the DumpFolder.")]
        public bool Dump { get; set; }

        [Category(Files), Description("Source文件夹:选择要分发的PKM文件的地方\nSource folder: where PKM files to distribute are selected from.")]
        public string DistributeFolder { get; set; } = string.Empty;

        [Category(Files), Description("Destination文件夹:所有接收到的PKM文件被转储到的地方\nDestination folder: where all received PKM files are dumped to.")]
        public string DumpFolder { get; set; } = string.Empty;

        [Category(Files), Description("批量交换根目录")]
        public string TradeFolder { get; set; } = string.Empty;

        [Category(Files), Description("截图根目录")]
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
        }
    }
}