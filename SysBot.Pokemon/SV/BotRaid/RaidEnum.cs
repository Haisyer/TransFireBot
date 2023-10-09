using System.Collections.Generic;

namespace SysBot.Pokemon
{
    public enum DTFormat
    {
        MMDDYY,
        DDMMYY,
        YYMMDD,
    }
    public enum TeraCrystalType : int
    {
        Base = 0,
        Black = 1,
        Distribution = 2,
        Might = 3,
    }
    public enum LobbyMethodOptions
    {
        OpenLobby,
        SkipRaid,
        ContinueRaid,
    }

    public class BanList
    {
        public bool enabled { get; set; }
        public ulong[] NIDs { get; set; } = { };
        public string Names { get; set; } = string.Empty;
        public ulong[] DiscordIDs { get; set; } = { };
        public string Comment { get; set; } = string.Empty;
    }
}