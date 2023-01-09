using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon
{
    public class StopConditionSettings
    {
        private const string StopConditions = nameof(StopConditions);
        public override string ToString() => "停止条件设置Stop Condition Settings";

        [Category(StopConditions), Description("只停在这个物种的神奇宝贝上。如果设置为 \"无 \"则没有限制\nStops only on Pokémon of this species. No restrictions if set to \"None\".")]
        public Species StopOnSpecies { get; set; }

        [Category(StopConditions), Description("只停在具有FormID的神奇宝贝上。如果留空则没有限制\nStops only on Pokémon with this FormID. No restrictions if left blank.")]
        public int? StopOnForm { get; set; }

        [Category(StopConditions), Description("只停在指定性格的神奇宝贝上\nStop only on Pokémon of the specified nature.")]
        public Nature TargetNature { get; set; } = Nature.Random;

        [Category(StopConditions), Description("最小接受的IV，格式为HP/Atk/Def/SpA/SpD/Spe。使用 \"x\"表示未检查的IV，使用 \"/\"作为分隔符\nMinimum accepted IVs in the format HP/Atk/Def/SpA/SpD/Spe. Use \"x\" for unchecked IVs and \"/\" as a separator.")]
        public string TargetMinIVs { get; set; } = "";

        [Category(StopConditions), Description("最大接受的IV的格式为HP/Atk/Def/SpA/SpD/Spe。使用 \"x\"表示未选中的IV，使用 \"/\"作为分隔符\nMaximum accepted IVs in the format HP/Atk/Def/SpA/SpD/Spe. Use \"x\" for unchecked IVs and \"/\" as a separator.")]
        public string TargetMaxIVs { get; set; } = "";

        [Category(StopConditions), Description("选择闪亮的类型来停止\nSelects the shiny type to stop on.")]
        public TargetShinyType ShinyTarget { get; set; } = TargetShinyType.DisableOption;

        [Category(StopConditions), Description("只停在有证章的神奇宝贝上\nStop only on Pokémon that have a mark.")]
        public bool MarkOnly { get; set; } = false;

        [Category(StopConditions), Description("用逗号分隔的要忽略的标记列表。使用全称，例如：\"Uncommon Mark, Dawn Mark, Prideful Mark/\"\nList of marks to ignore separated by commas. Use the full name, e.g. \"Uncommon Mark, Dawn Mark, Prideful Mark\".")]
        public string UnwantedMarks { get; set; } = "";

        [Category(StopConditions), Description("当遭遇机器人或化石机器人(仅在剑/盾中起作用)发现匹配的神奇宝贝时，按住捕获按钮，录制30秒的片段\nHolds Capture button to record a 30 second clip when a matching Pokémon is found by EncounterBot or Fossilbot.")]
        public bool CaptureVideoClip { get; set; }

        [Category(StopConditions), Description("遭遇机器人或化石机器人(仅在剑/盾中起作用)在匹配后，在按下捕获键之前要等待的额外时间，单位为毫秒\nExtra time in milliseconds to wait after an encounter is matched before pressing Capture for EncounterBot or Fossilbot.")]
        public int ExtraTimeWaitCaptureVideo { get; set; } = 10000;

        [Category(StopConditions), Description("如果设置为TRUE，则同时匹配闪光类型和目标个体值设置。否则，寻找闪光类型或目标个体值的匹配\nIf set to TRUE, matches both ShinyTarget and TargetIVs settings. Otherwise, looks for either ShinyTarget or TargetIVs match.")]
        public bool MatchShinyAndIV { get; set; } = true;

        [Category(StopConditions), Description("如果不是空的，所提供的字符串将被添加到发现结果的日志信息中，以对你指定的人进行Echo提醒。对于Discord，使用<@userIDnumber>来提及\nIf not empty, the provided string will be prepended to the result found log message to Echo alerts for whomever you specify. For Discord, use <@userIDnumber> to mention.")]
        public string MatchFoundEchoMention { get; set; } = string.Empty;

        public static bool EncounterFound<T>(T pk, int[] targetminIVs, int[] targetmaxIVs, StopConditionSettings settings, IReadOnlyList<string>? marklist) where T : PKM
        {
            // Match Nature and Species if they were specified.
            if (settings.StopOnSpecies != Species.None && settings.StopOnSpecies != (Species)pk.Species)
                return false;

            if (settings.StopOnForm.HasValue && settings.StopOnForm != pk.Form)
                return false;

            if (settings.TargetNature != Nature.Random && settings.TargetNature != (Nature)pk.Nature)
                return false;

            // Return if it doesn't have a mark or it has an unwanted mark.
            var unmarked = pk is IRibbonIndex m && !HasMark(m);
            var unwanted = marklist is not null && pk is IRibbonIndex m2 && settings.IsUnwantedMark(GetMarkName(m2), marklist);
            if (settings.MarkOnly && (unmarked || unwanted))
                return false;

            if (settings.ShinyTarget != TargetShinyType.DisableOption)
            {
                bool shinymatch = settings.ShinyTarget switch
                {
                    TargetShinyType.AnyShiny => pk.IsShiny,
                    TargetShinyType.NonShiny => !pk.IsShiny,
                    TargetShinyType.StarOnly => pk.IsShiny && pk.ShinyXor != 0,
                    TargetShinyType.SquareOnly => pk.ShinyXor == 0,
                    TargetShinyType.DisableOption => true,
                    _ => throw new ArgumentException(nameof(TargetShinyType)),
                };

                // If we only needed to match one of the criteria and it shinymatch'd, return true.
                // If we needed to match both criteria and it didn't shinymatch, return false.
                if (!settings.MatchShinyAndIV && shinymatch)
                    return true;
                if (settings.MatchShinyAndIV && !shinymatch)
                    return false;
            }

            // Reorder the speed to be last.
            Span<int> pkIVList = stackalloc int[6];
            pk.GetIVs(pkIVList);
            (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);

            for (int i = 0; i < 6; i++)
            {
                if (targetminIVs[i] > pkIVList[i] || targetmaxIVs[i] < pkIVList[i])
                    return false;
            }
            return true;
        }

        public static void InitializeTargetIVs(PokeTradeHubConfig config, out int[] min, out int[] max)
        {
            min = ReadTargetIVs(config.StopConditions, true);
            max = ReadTargetIVs(config.StopConditions, false);
        }

        private static int[] ReadTargetIVs(StopConditionSettings settings, bool min)
        {
            int[] targetIVs = new int[6];
            char[] split = { '/' };

            string[] splitIVs = min
                ? settings.TargetMinIVs.Split(split, StringSplitOptions.RemoveEmptyEntries)
                : settings.TargetMaxIVs.Split(split, StringSplitOptions.RemoveEmptyEntries);

            // Only accept up to 6 values.  Fill it in with default values if they don't provide 6.
            // Anything that isn't an integer will be a wild card.
            for (int i = 0; i < 6; i++)
            {
                if (i < splitIVs.Length)
                {
                    var str = splitIVs[i];
                    if (int.TryParse(str, out var val))
                    {
                        targetIVs[i] = val;
                        continue;
                    }
                }
                targetIVs[i] = min ? 0 : 31;
            }
            return targetIVs;
        }

        private static bool HasMark(IRibbonIndex pk)
        {
            for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
            {
                if (pk.GetRibbon((int)mark))
                    return true;
            }
            return false;
        }

        public string GetPrintName(PKM pk)
        {
            var set = ShowdownParsing.GetShowdownText(pk);
            if (pk is IRibbonIndex r)
            {
                var rstring = GetMarkName(r);
                if (!string.IsNullOrEmpty(rstring))
                    set += $"\nPokémon found to have ***{GetMarkName(r)}***!\n发现有证章的神奇宝贝";
            }
            return set;
        }

        public static void ReadUnwantedMarks(StopConditionSettings settings, out IReadOnlyList<string> marks) =>
            marks = settings.UnwantedMarks.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

        public virtual bool IsUnwantedMark(string mark, IReadOnlyList<string> marklist) => marklist.Contains(mark);

        public static string GetMarkName(IRibbonIndex pk)
        {
            for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
            {
                if (pk.GetRibbon((int)mark))
                    return RibbonStrings.GetName($"Ribbon{mark}");
            }
            return "";
        }
    }

    public enum TargetShinyType
    {
        DisableOption,  // Doesn't care
        NonShiny,       // Match nonshiny only
        AnyShiny,       // Match any shiny regardless of type
        StarOnly,       // Match star shiny only
        SquareOnly,     // Match square shiny only
    }
}
