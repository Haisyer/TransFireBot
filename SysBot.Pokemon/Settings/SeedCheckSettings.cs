using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class SeedCheckSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "种子检索设置";

        [Category(FeatureToggle), Description("启用后，种子检索将返还所有可能的种子结果，而不是只返还第一个有效的匹配结果。")]
        public bool ShowAllZ3Results { get; set; }

        [Category(FeatureToggle), Description("只允许返还最近的闪光帧，第一个星形和方形闪光帧，或是前三个闪光帧。")]
        public SeedCheckResults ResultDisplayMode { get; set; }
    }

    public enum SeedCheckResults
    {
        ClosestOnly,            // Only gets the first shiny,只得到第一个闪亮的
        FirstStarAndSquare,     // Gets the first star shiny and first square shiny,获得第一颗星闪和方块闪
        FirstThree,             // Gets the first three frames,获取前三帧
    }
}
