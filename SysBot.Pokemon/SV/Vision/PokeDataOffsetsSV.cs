using System.Collections.Generic;

namespace SysBot.Pokemon
{
    /// <summary>
    /// Scarlet/Violet: RAM offsets
    /// </summary>
    public class PokeDataOffsetsSV
    {
        public const string SVGameVersion = "2.0.1";
        public const string ScarletID = "0100A3D008C5C000";
        public const string VioletID = "01008F6008C5E000";
        public IReadOnlyList<long> BoxStartPokemonPointer { get; } = new long[] { 0x46445E8, 0xA90, 0x9D0, 0x0 };
        public IReadOnlyList<long> LinkTradePartnerPokemonPointer { get; } = new long[] { 0x461AE58, 0x48, 0x58, 0x40, 0x148 };
        public IReadOnlyList<long> LinkTradePartnerNIDPointer { get; } = new long[] { 0x463F4B8, 0xF8, 0x08 };
        public IReadOnlyList<long> MyStatusPointer { get; } = new long[] { 0x46215F0, 0x10, 0x40 };
        public IReadOnlyList<long> Trader1MyStatusPointer { get; } = new long[] { 0x461AE58, 0x48, 0xB0, 0x0 }; // The trade partner status uses a compact struct that looks like MyStatus.
        public IReadOnlyList<long> Trader2MyStatusPointer { get; } = new long[] { 0x461AE58, 0x48, 0xE0, 0x0 };
        public IReadOnlyList<long> ConfigPointer { get; } = new long[] { 0x46213A8, 0x10, 0x40 };
        public IReadOnlyList<long> CurrentBoxPointer { get; } = new long[] { 0x46447D0, 0xF0, 0x50, 0x548 };
        public IReadOnlyList<long> PortalBoxStatusPointer { get; } = new long[] { 0x4690668, 0x740, 0x530, 0x6C8 };  // 9-A in portal, 4-6 in box.
        public IReadOnlyList<long> IsConnectedPointer { get; } = new long[] { 0x4644948, 0x18 };
        public IReadOnlyList<long> OverworldPointer { get; } = new long[] { 0x4644870, 0x348, 0x10, 0xD8, 0x28 };

        public const int BoxFormatSlotSize = 0x158;
        public const ulong LibAppletWeID = 0x010000000000100a; // One of the process IDs for the news.
        public IReadOnlyList<long> TeraRaidCodePointer { get; } = new long[] { 0x44DDC10, 0x10, 0x78, 0x10, 0x1A9 };
        public IReadOnlyList<long> TeraRaidBlockPointer { get; } = new long[] { 0x44BFBA8, 0x180, 0x40 };

        public ulong TeraLobby { get; } = 0x04174430;
        public ulong LoadedIntoRaid { get; } = 0x04551020;
    }
}
