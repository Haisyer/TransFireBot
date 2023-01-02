using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class SeedCheckSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "种子检查设置Seed Check Settings";

        [Category(FeatureToggle), Description("启用后，种子检查将返回所有可能的种子结果，而不是第一个有效的匹配\nWhen enabled, seed checks will return all possible seed results instead of the first valid match.")]
        public bool ShowAllZ3Results { get; set; }

        [Category(FeatureToggle), Description("只允许返回最接近的闪亮框架，第一个星星和方形闪亮框架，或前三个闪亮框架\nAllows returning only the closest shiny frame, the first star and square shiny frames, or the first three shiny frames.")]
        public SeedCheckResults ResultDisplayMode { get; set; }
    }

    public enum SeedCheckResults
    {
        ClosestOnly,            // Only gets the first shiny,只得到第一个闪亮的
        FirstStarAndSquare,     // Gets the first star shiny and first square shiny,获得第一颗星闪和方块闪
        FirstThree,             // Gets the first three frames,获取前三帧
    }
}
