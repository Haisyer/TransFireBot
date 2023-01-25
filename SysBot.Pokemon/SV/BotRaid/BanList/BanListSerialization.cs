
namespace SysBot.Pokemon.SV
{
    public class BanListSerialization
    {
        protected class BanCheckResult
        {
            public string RaiderName { get; set; } = string.Empty;
            public string BannedUserName { get; set; } = string.Empty;
            public string BanReason { get; set; } = string.Empty;
            public int LevenshteinDistance { get; set; }
            public double Log10p { get; set; }
            public ResultType MatchType { get; set; }
            public bool IsBanned { get; set; }
        }

        protected class LanguageData
        {
            public string Name { get; set; } = string.Empty;
            public string Language { get; set; } = string.Empty;
            public string Weight { get; set; } = string.Empty;
        }

        protected class BannedRaider
        {
            public string Name { get; set; } = string.Empty;
            public bool Enabled { get; set; }
            public string Language { get; set; } = string.Empty;
            public double Log10p { get; set; }
            public string Notes { get; set; } = string.Empty;
        }
    }

    public enum ResultType
    {
        IS_EXACTMATCH = 0,
        IS_SIMILAR_MATCH = 1,
        NO_MATCH = 2,
    }
}
