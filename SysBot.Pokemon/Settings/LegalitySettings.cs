using PKHeX.Core;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class LegalitySettings
    {
        private string DefaultTrainerName = "SysBot";
        private const string Generate = nameof(Generate);
        private const string Misc = nameof(Misc);
        public override string ToString() => "合法性生成设置Legality Generating Settings";

        // Generate
        [Category(Generate), Description("奇迹卡的MGDB目录路径\nMGDB directory path for Wonder Cards.")]
        public string MGDBPath { get; set; } = string.Empty;

        [Category(Generate), Description("用于重新生成PKM文件的训练家数据的文件夹\nFolder for PKM files with trainer data to use for regenerated PKM files.")]
        public string GeneratePathTrainerInfo { get; set; } = string.Empty;

        [Category(Generate), Description("PKM文件的默认原始训练家名称与提供的任何PKM文件都不匹配。\nDefault Original Trainer name for PKM files that don't match any of the provided PKM files.")]
        public string GenerateOT
        {
            get => DefaultTrainerName;
            set
            {
                if (!StringsUtil.IsSpammyString(value))
                    DefaultTrainerName = value;
            }
        }

        [Category(Generate), Description("如果请求与提供的任何Trainer数据文件都不匹配，则默认的16位Trainer ID (TID)。这应该是一个5位数\nDefault 16-bit Trainer ID (TID) for requests that don't match any of the provided trainer data files. This should be a 5-digit number.")]
        public ushort GenerateTID16 { get; set; } = 12345;

        [Category(Generate), Description("如果请求与提供的任何训练器数据文件都不匹配，则使用默认的16位里ID (SID)。这应该是一个5位数\nDefault 16-bit Secret ID (SID) for requests that don't match any of the provided trainer data files. This should be a 5-digit number.")]
        public ushort GenerateSID16 { get; set; } = 54321;

        [Category(Generate), Description("PKM文件的默认语言，不匹配任何提供的PKM文件\nDefault language for PKM files that don't match any of the provided PKM files.")]
        public LanguageID GenerateLanguage { get; set; } = LanguageID.English;

        [Category(Generate), Description("如果优先级游戏设置为\"true \"，则使用优先考虑游戏版本开始创建来源版本。如果\"false\"，则使用最新的游戏作为版本\nIf PrioritizeGame is set to \"true\", uses PrioritizeGameVersion to start looking for encounters. If \"false\", uses newest game as the version.")]
        public bool PrioritizeGame { get; set; } = true;

        [Category(Generate), Description("指定在检查其他游戏之前创建来源版本的第一个游戏，或者如果该字段被设置为\"Any\"，则指定当前游戏。将优先级游戏设置为\"true\"，表示启用\nSpecifies the first game to try and generate encounters before checking other games, or current game if this field is set to \"Any\". Set PrioritizeGame to \"true\" to enable.")]
        public GameVersion PrioritizeGameVersion { get; set; } = GameVersion.Any;

        [Category(Generate), Description("为任何生成的Pokémon设置所有可能的合法证章\nSet all possible legal ribbons for any generated Pokémon.")]
        public bool SetAllLegalRibbons { get; set; }

        [Category(Generate), Description("为任何生成的Pokémon设置一个匹配的球(基于宝可梦颜色)\nSet a matching ball (based on color) for any generated Pokémon.")]
        public bool SetMatchingBalls { get; set; }

        [Category(Generate), Description("如果合法，强制使用指定的球\nForce the specified ball if legal.")]
        public bool ForceSpecifiedBall { get; set; } = false;

        [Category(Generate), Description("根据游戏RNG生成合法、非异色、8代、来自宝可梦巢穴的宝可梦。\nAllow XOROSHIRO when generating Gen 8 Raid Pokémon.")]
        public bool UseXOROSHIRO { get; set; } = true;

        [Category(Generate), Description("是否为用户返回其中文指令生成的ShowDown代码,暂只有dodo可用")]
        public bool ReturnShowdownSets { get; set; } = true;

        [Category(Generate), Description("是否允许用户使用文件,暂只有dodo可用")]
        public bool AllowUseFile { get; set; } = true;

        [Category(Generate), Description("增加对战版本，支持它(目前仅仅支持SWSH)使用过去的一代Pokémon在线对战游戏\nAdds Battle Version for games that support it (SWSH only) for using past-gen Pokémon in online competitive play.")]
        public bool SetBattleVersion { get; set; } = false;

        [Category(Generate), Description("如果提供了非法的SET代码，机器人将创建一个复活节Pokémon蛋\nBot will create an Easter Egg Pokémon if provided an illegal set.")]
        public bool EnableEasterEggs { get; set; } = false;

        [Category(Generate), Description("允许用户在Showdown sets中提交自定义的OT, TID, SID和OT Gender,即允许用户使用PS代码实现自ID\nAllow users to submit custom OT, TID, SID, and OT Gender in Showdown sets.")]
        public bool AllowTrainerDataOverride { get; set; } = false;

        [Category(Generate), Description("允许用户使用Pkhex批处理编辑命令实现进一步的定制\nAllow users to submit further customization with Batch Editor commands.")]
        public bool AllowBatchCommands { get; set; } = false;

        [Category(Generate), Description("在取消之前生成SET代码时所花费的最大时间(以秒为单位)。这可以防止复杂的代码冻结机器人\nMaximum time in seconds to spend when generating a set before canceling. This prevents difficult sets from freezing the bot.")]
        public int Timeout { get; set; } = 15;

        [Category(Generate), Description("允许指令非法开关")]
        public bool CommandillegalMod { get; set; } = false;

        [Category(Generate), Description("允许文件非法开关")]
        public bool FileillegalMod { get; set; } = false;

        // Misc

        [Category(Misc), Description("将HOME跟踪器归零，不管当前跟踪器的值。同样适用于用户要求的PKM文件\nZero out HOME tracker regardless of current tracker value. Applies to user requested PKM files as well.")]
        public bool ResetHOMETracker { get; set; } = true;

        [Category(Misc), Description("由交换对象重写训练家信息\nOverride trainer data by trade partner")]
        public bool UseTradePartnerInfo { get; set; } = false;
    }
}
