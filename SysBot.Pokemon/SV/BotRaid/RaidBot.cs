using System;
using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Pokemon.SV;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Discord;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.RaidSettingsSV;
using System.Net.Http;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class RaidSV : PokeRoutineExecutor9SV, ICountBot
    {
        private readonly PokeTradeHub<PK9> Hub;
        private readonly RaidSettingsSV Settings;
        public ICountSettings Counts => Settings;

        public static CancellationTokenSource RaidSVEmbedSource { get; set; } = new();
        public static bool RaidSVEmbedsInitialized { get; set; }
      //  public static ConcurrentQueue<(byte[]?, EmbedBuilder)> EmbedQueue { get; set; } = new();
        private RemoteControlAccessList RaiderBanList => Settings.RaiderBanList;

        public RaidSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
        {
            Hub = hub;
            Settings = hub.Config.RaidSV;
        }

        private const string RaidBotVersion = "Version 0.4.1";
        private int RaidsAtStart;
        private int RaidCount;
        private int WinCount;
        private int LossCount;
        private readonly Dictionary<ulong, int> RaidTracker = new();
        private SAV9SV HostSAV = new();
        private DateTime StartTime = DateTime.Now;

        private ulong TodaySeed;
        private ulong OverworldOffset;
        private ulong ConnectedOffset;
        private ulong TeraRaidBlockOffset;
        private readonly ulong[] TeraNIDOffsets = new ulong[3];        

        public override async Task MainLoop(CancellationToken token)
        {
            if (Settings.ConfigureRolloverCorrection)
            {
                await RolloverCorrectionSV(token).ConfigureAwait(false);
                return;
            }

            if (Settings.TimeToWait is < 0 or > 180)
            {
                Log("Time to wait must be between 0 and 180 seconds.");
                return;
            }

            if (Settings.RaidsBetweenUpdate == 0 || Settings.RaidsBetweenUpdate < -1)
            {
                Log("Raids between updating the global ban list must be greater than 0, or -1 if you want it off.");
                return;
            }

            try
            {
                Log("Identifying trainer data of the host console.");
                HostSAV = await IdentifyTrainer(token).ConfigureAwait(false);
                await InitializeHardware(Settings, token).ConfigureAwait(false);

                Log("Starting main RaidBot loop.");
                await InnerLoop(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            Log($"Ending {nameof(RaidBotSWSH)} loop.");
            await HardStop().ConfigureAwait(false);
        }

        private async Task InnerLoop(CancellationToken token)
        {
            bool partyReady;
            List<(ulong, TradeMyStatus)> lobbyTrainers;
            StartTime = DateTime.Now;
            var dayRoll = 0;
            while (!token.IsCancellationRequested)
            {
                // Initialize offsets at the start of the routine and cache them.
                await InitializeSessionOffsets(token).ConfigureAwait(false);
                if (RaidCount == 0)
                {
                    TodaySeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 8, token).ConfigureAwait(false), 0);
                    Log($"Starting routine with Today Seed: {TodaySeed:X8}");
                }

                var currentSeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 8, token).ConfigureAwait(false), 0);
                if (TodaySeed != currentSeed)
                {
                    if (dayRoll != 0)
                    {
                        Log($"Current Today Seed {currentSeed:X8} does not match Starting Today Seed: {TodaySeed:X8} after rolling back 1 day. Stopping routine for lost raid.");
                        return;
                    }
                    Log($"Current Today Seed {currentSeed:X8} does not match Starting Today Seed: {TodaySeed:X8}, attempting dayroll correction.");
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await RolloverCorrectionSV(token).ConfigureAwait(false);
                    await StartGame(Hub.Config, token).ConfigureAwait(false);

                    dayRoll++;
                    continue;
                }

                // Get initial raid counts for comparison later.
                await CountRaids(null, token).ConfigureAwait(false);

                // Clear NIDs.
                await SwitchConnection.WriteBytesAbsoluteAsync(new byte[32], TeraNIDOffsets[0], token).ConfigureAwait(false);

                // Connect online and enter den.
                if (!await PrepareForRaid(token).ConfigureAwait(false))
                {
                    Log("Failed to prepare the raid, rebooting the game.");
                    await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                    continue;
                }

                // Wait until we're in lobby.
                if (!await GetLobbyReady(token).ConfigureAwait(false))
                    continue;

                // Read trainers until someone joins.
                (partyReady, lobbyTrainers) = await ReadTrainers(token).ConfigureAwait(false);
                if (!partyReady)
                {
                    // Should add overworld recovery with a game restart fallback.
                    await RegroupFromBannedUser(token).ConfigureAwait(false);

                    if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    {
                        Log("Something went wrong, attempting to recover.");
                        await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                        continue;
                    }

                    // Clear trainer OTs.
                    Log("Clearing stored OTs");
                    for (int i = 0; i < 3; i++)
                    {
                        var ptr = new long[] { 0x44B97D8, 0x48, 0xE0 + (i * 0x30), 0x0 };
                        await SwitchConnection.PointerPoke(new byte[16], ptr, token).ConfigureAwait(false);
                    }
                    continue;
                }

                await CompleteRaid(lobbyTrainers, token).ConfigureAwait(false);
            }
        }

        public override async Task HardStop()
        {
            RaidSVEmbedsInitialized = false;
            RaidSVEmbedSource.Cancel();
            await CleanExit(CancellationToken.None).ConfigureAwait(false);
        }

        private async Task CompleteRaid(List<(ulong, TradeMyStatus)> trainers, CancellationToken token)
        {
            List<(ulong, TradeMyStatus)> lobbyTrainersFinal = new();
            if (await IsConnectedToLobby(token).ConfigureAwait(false))
            {
                int b = 0;
                Log("Preparing for battle!");
                while (!await IsInRaid(token).ConfigureAwait(false))
                    await Click(A, 1_000, token).ConfigureAwait(false);

                if (await IsInRaid(token).ConfigureAwait(false))
                {
                    // Clear NIDs to refresh player check.
                    await SwitchConnection.WriteBytesAbsoluteAsync(new byte[32], TeraNIDOffsets[0], token).ConfigureAwait(false);
                    await Task.Delay(5_000, token).ConfigureAwait(false);

                    // Loop through trainers again in case someone disconnected.
                    for (int i = 0; i < 3; i++)
                    {
                        var player = i + 2;
                        var nidOfs = TeraNIDOffsets[i];
                        var data = await SwitchConnection.ReadBytesAbsoluteAsync(nidOfs, 8, token).ConfigureAwait(false);
                        var nid = BitConverter.ToUInt64(data, 0);

                        if (nid == 0)
                            continue;

                        var pointer = new long[] { 0x44B97D8, 0x48, 0xE0 + (i * 0x30), 0x0 };
                        var trainer = await GetTradePartnerMyStatus(pointer, token).ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(trainer.OT))
                            continue;

                        lobbyTrainersFinal.Add((nid, trainer));
                        var tr = trainers.FirstOrDefault(x => x.Item2.OT == trainer.OT);
                        if (tr != default)
                            Log($"Player {i + 2} matches lobby check for {trainer.OT}.");
                        else Log($"New Player {i + 2}: {trainer.OT} | TID: {trainer.DisplayTID} | NID: {nid}.");
                    }
                    var nidDupe = lobbyTrainersFinal.Select(x => x.Item1).ToList();
                    var dupe = lobbyTrainersFinal.Count > 1 && nidDupe.Distinct().Count() == 1;
                    if (dupe)
                    {
                        // We read bad data, reset game to end early and recover.
                        var msg = "Oops! Something went wrong, resetting to recover.";
                        await EnqueueEmbed(null, msg, false, false, false, token).ConfigureAwait(false);
                        await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                        return;
                    }

                    var names = lobbyTrainersFinal.Select(x => x.Item2.OT).ToList();
                    bool hatTrick = lobbyTrainersFinal.Count == 3 && names.Distinct().Count() == 1;

                    await Task.Delay(15_000, token).ConfigureAwait(false);
                    await EnqueueEmbed(names, "", hatTrick, false, true, token).ConfigureAwait(false);
                }

                while (await IsConnectedToLobby(token).ConfigureAwait(false))
                {
                    b++;
                    await Click(A, 3_000, token).ConfigureAwait(false);
                    if (b % 10 == 0)
                        Log("Still in battle...");
                }
            }

            Log("Raid lobby disbanded!");
            await Click(B, 0_500, token).ConfigureAwait(false);
            await Click(B, 0_500, token).ConfigureAwait(false);
            await Click(DDOWN, 0_500, token).ConfigureAwait(false);

            Log("Returning to overworld...");
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Click(A, 1_000, token).ConfigureAwait(false);

            await CountRaids(lobbyTrainersFinal, token).ConfigureAwait(false);

            await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
        }

        private void ApplyPenalty(List<(ulong, TradeMyStatus)> trainers)
        {
            for (int i = 0; i < trainers.Count; i++)
            {
                var nid = trainers[i].Item1;
                var name = trainers[i].Item2.OT;
                if (RaidTracker.ContainsKey(nid) && nid != 0)
                {
                    var entry = RaidTracker[nid];
                    var penalty = entry + 1;
                    RaidTracker[nid] = penalty;
                    Log($"Player: {name} completed the raid with Penalty Count: {penalty}.");

                    if (penalty > Settings.CatchLimit && !RaiderBanList.Contains(nid) && Settings.CatchLimit != 0)
                    {
                        Log($"Player: {name} added to ban list as of {penalty}/{Settings.CatchLimit} this raid session on {DateTime.Now}.");
                        RaiderBanList.List.Add(new() { ID = nid, Name = name, Comment = $"{Settings.RaidSpecies} ({penalty}/{Settings.CatchLimit}) @ {DateTime.Now}." });
                    }
                }
            }
        }

        private async Task CountRaids(List<(ulong, TradeMyStatus)>? trainers, CancellationToken token)
        {
            List<uint> seeds = new();
            var data = await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset + 0x20, 2304, token).ConfigureAwait(false);
            for (int i = 0; i < 69; i++)
            {
                var seed = BitConverter.ToUInt32(data.Slice(0 + (i * 32), 4));
                if (seed != 0)
                    seeds.Add(seed);
            }

            Log($"Active raid count: {seeds.Count}");
            if (RaidCount == 0)
            {
                RaidsAtStart = seeds.Count;
                return;
            }

            if (trainers is not null)
            {
                Log("Back in the overworld, checking if we won or lost.");
                Settings.AddCompletedRaids();
                if (RaidsAtStart > seeds.Count)
                {
                    Log("We defeated the raid boss!");
                    WinCount++;
                    if (trainers.Count > 0)
                        ApplyPenalty(trainers);
                    return;
                }

                Log("We lost the raid...");
                LossCount++;
            }
        }

        private async Task<bool> PrepareForRaid(CancellationToken token)
        {
            Log("Preparing lobby...");

            // Make sure we're connected.
            while (!await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
            {
                Log("Connecting...");
                await RecoverToOverworld(token).ConfigureAwait(false);
                if (!await ConnectToOnline(Hub.Config, token).ConfigureAwait(false))
                    return false;
            }

            for (int i = 0; i < 5; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            // If not in the overworld, we've been attacked so quit earlier.
            if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                return false;

            await Click(A, 3_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);

            if (!Settings.CodeTheRaid)
                await Click(DDOWN, 1_000, token).ConfigureAwait(false);

            await Click(A, 8_000, token).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> GetLobbyReady(CancellationToken token)
        {
            var x = 0;
            Log("Connecting to lobby...");
            while (!await IsConnectedToLobby(token).ConfigureAwait(false))
            {
                await Click(A, 1_000, token).ConfigureAwait(false);
                x++;
                if (x == 45)
                {
                    Log("Failed to connect to lobby, restarting game incase we were in battle/bad connection.");
                    await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                    Log("Attempting to restart routine!");
                    return false;
                }
            }
            return true;
        }

        private async Task<string> GetRaidCode(CancellationToken token)
        {
            var data = await SwitchConnection.PointerPeek(6, Offsets.TeraRaidCodePointer, token).ConfigureAwait(false);
            string str = Encoding.ASCII.GetString(data);

            Log($"Raid Code: {str}");
            return $"{str}";
        }

        private async Task<bool> CheckIfTrainerBanned(TradeMyStatus trainer, ulong nid, int player, bool updateBanList, CancellationToken token)
        {
            Log($"Player {player}: {trainer.OT} | TID: {trainer.DisplayTID} | NID: {nid}");
            if (!RaidTracker.ContainsKey(nid))
                RaidTracker.Add(nid, 0);

            var banResultCC = Settings.RaidsBetweenUpdate == -1 ? (false, "") : await BanService.IsRaiderBanned(trainer.OT, Settings.BanListURL, Connection.Label, updateBanList).ConfigureAwait(false);
            var banResultCFW = RaiderBanList.List.FirstOrDefault(x => x.ID == nid);

            bool isBanned = banResultCC.Item1 || banResultCFW != default;
            if (isBanned)
            {
                var msg = banResultCC.Item1 ? banResultCC.Item2 : $"Banned user {banResultCFW!.Name} found in the host's ban list.\n{banResultCFW.Comment}";
                Log(msg);

                await EnqueueEmbed(null, msg, false, true, false, token).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        // This is messy, needs a way to check if player X is ready, and when we're in a raid, in order to avoid adding players that may have disconnected or quit. Players get shifted down as they leave.
        private async Task<(bool, List<(ulong, TradeMyStatus)>)> ReadTrainers(CancellationToken token)
        {
            await EnqueueEmbed(null, "", false, false, false, token).ConfigureAwait(false);

            List<(ulong, TradeMyStatus)> lobbyTrainers = new();
            var wait = TimeSpan.FromSeconds(Settings.TimeToWait);
            var endTime = DateTime.Now + wait;
            bool full = false;
            bool updateBanList = Settings.RaidsBetweenUpdate != -1 && (RaidCount == 0 || RaidCount % Settings.RaidsBetweenUpdate == 0);

            while (!full && (DateTime.Now < endTime))
            {
                // Loop through trainers
                for (int i = 0; i < 3; i++)
                {
                    var player = i + 2;
                    Log($"Waiting for Player {player} to load...");

                    var nidOfs = TeraNIDOffsets[i];
                    var data = await SwitchConnection.ReadBytesAbsoluteAsync(nidOfs, 8, token).ConfigureAwait(false);
                    var nid = BitConverter.ToUInt64(data, 0);
                    while (nid == 0 && (DateTime.Now < endTime))
                    {
                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        data = await SwitchConnection.ReadBytesAbsoluteAsync(nidOfs, 8, token).ConfigureAwait(false);
                        nid = BitConverter.ToUInt64(data, 0);
                    }

                    var pointer = new long[] { 0x44B97D8, 0x48, 0xE0 + (i * 0x30), 0x0 };
                    var trainer = await GetTradePartnerMyStatus(pointer, token).ConfigureAwait(false);

                    while (trainer.OT.Length == 0 && (DateTime.Now < endTime))
                    {
                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        trainer = await GetTradePartnerMyStatus(pointer, token).ConfigureAwait(false);
                    }

                    if (nid != 0 && !string.IsNullOrWhiteSpace(trainer.OT))
                    {
                        if (await CheckIfTrainerBanned(trainer, nid, player, updateBanList, token).ConfigureAwait(false))
                            return (false, lobbyTrainers);

                        updateBanList = false;
                    }

                    if (lobbyTrainers.FirstOrDefault(x => x.Item1 == nid) != default && trainer.OT.Length > 0)
                        lobbyTrainers[i] = (nid, trainer);
                    else if (nid > 0 && trainer.OT.Length > 0)
                        lobbyTrainers.Add((nid, trainer));

                    full = lobbyTrainers.Count == 3;
                    if (full || (DateTime.Now >= endTime))
                        break;
                }
            }

            RaidCount++;
            if (lobbyTrainers.Count == 0)
            {
                Log("Nobody joined the raid, recovering...");
                return (false, lobbyTrainers);
            }
            Log($"Raid #{RaidCount} is starting!");
            return (true, lobbyTrainers);
        }

        private async Task<bool> IsConnectedToLobby(CancellationToken token)
        {
            var data = await SwitchConnection.ReadBytesMainAsync(Offsets.TeraLobby, 1, token).ConfigureAwait(false);
            return data[0] != 0x00; // 0 when in lobby but not connected
        }

        private async Task<bool> IsInRaid(CancellationToken token)
        {
            var data = await SwitchConnection.ReadBytesMainAsync(Offsets.LoadedIntoRaid, 1, token).ConfigureAwait(false);
            return data[0] == 0x02; // 2 when in raid, 1 when not
        }

        private async Task RolloverCorrectionSV(CancellationToken token)
        {
            var scrollroll = Settings.DateTimeFormat switch
            {
                DTFormat.DDMMYY => 0,
                DTFormat.YYMMDD => 2,
                _ => 1,
            };

            for (int i = 0; i < 2; i++)
                await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(A, 1_250, token).ConfigureAwait(false); // Enter settings

            await PressAndHold(DDOWN, 2_000, 0_250, token).ConfigureAwait(false); // Scroll to system settings
            await Click(A, 1_250, token).ConfigureAwait(false);

            await PressAndHold(DDOWN, Settings.HoldTimeForRollover, 0, token).ConfigureAwait(false);
            await Click(DUP, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_250, token).ConfigureAwait(false);
            for (int i = 0; i < 2; i++)
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);
            for (int i = 0; i < scrollroll; i++) // 0 to roll day for DDMMYY, 1 to roll day for MMDDYY, 3 to roll hour
                await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            for (int i = 0; i < 8; i++) // Mash DRIGHT to confirm
                await Click(DRIGHT, 0_200, token).ConfigureAwait(false);

            await Click(A, 0_200, token).ConfigureAwait(false); // Confirm date/time change
            await Click(HOME, 1_000, token).ConfigureAwait(false); // Back to title screen
        }

        private async Task RegroupFromBannedUser(CancellationToken token)
        {
            Log("Attempting to remake lobby..");
            await Click(B, 2_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);
            await Click(B, 1_000, token).ConfigureAwait(false);
        }

        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("Caching session offsets...");
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
            ConnectedOffset = await SwitchConnection.PointerAll(Offsets.IsConnectedPointer, token).ConfigureAwait(false);
            TeraRaidBlockOffset = await SwitchConnection.PointerAll(Offsets.TeraRaidBlockPointer, token).ConfigureAwait(false);

            var nidPointer = new long[] { Offsets.LinkTradePartnerNIDPointer[0], Offsets.LinkTradePartnerNIDPointer[1], Offsets.LinkTradePartnerNIDPointer[2] };
            for (int p = 0; p < TeraNIDOffsets.Length; p++)
            {
                nidPointer[2] = Offsets.LinkTradePartnerNIDPointer[2] + (p * 0x8);
                TeraNIDOffsets[p] = await SwitchConnection.PointerAll(nidPointer, token).ConfigureAwait(false);
            }
            EchoUtil.Echo("Caching offsets complete!");
            Log("Caching offsets complete!");
        }
        //需要修改一下
        private async Task EnqueueEmbed(List<string>? names, string message, bool hatTrick, bool disband, bool starting, CancellationToken token)
        {
            
            {
                // Get Code early for use in title IF starting
                var rcode = await GetRaidCode(token).ConfigureAwait(false);
                // Title can only be up to 256 characters.
                // IF hatTrick & IF NOT empty names
                // IF RaidEmbedTitle > 0 & raid starting
                // IF RaidEmbedTitle > 0
                var title = hatTrick && names is not null && names[0] == "Alice" ? $"**💜💜💜 🪄🎩✨ Thine beloved {names[0]} with the Hat Trick ✨🎩🪄 💜💜💜**" : hatTrick && names is not null && names[0] == "Alice" ? $"**🪄🎩✨ {names[0]} with the Hat Trick! ✨🎩🪄**" : Settings.RaidEmbedTitle.Length > 0 && starting ? $"**Raid: {RaidCount} Starting! [{rcode}]**" : $"**{Settings.RaidEmbedTitle} [Limit: {Settings.CatchLimit}]**";
                if (title.Length > 256)
                    title = title[..256];

                // Description can only be up to 4096 characters.
                var description = Settings.RaidEmbedDescription.Length > 0 ? string.Join("\n", Settings.RaidEmbedDescription) : "";
                if (description.Length > 4096)
                    description = description[..4096];

                byte[]? bytes = Array.Empty<byte>();
                if (Settings.TakeScreenshot)
                    bytes = await SwitchConnection.Screengrab(token).ConfigureAwait(false) ?? Array.Empty<byte>();
                /*      var embed = new EmbedBuilder()
                      {
                          Title = disband ? "**Raid was disbanded due to a banned user**" : title,
                          Description = disband ? message : !starting ? description + $"᲼\n᲼" : description,
                          Color = disband ? Color.Red : hatTrick ? Color.DarkMagenta : starting ? Color.Purple : Color.Gold,
                          ImageUrl = bytes.Length > 0 ? "attachment://zap.jpg" : default,
                      }.WithFooter(new EmbedFooterBuilder()
                      {
                          Text = $"Raids: {WinCount + LossCount} - Wins: {WinCount} - Losses: {LossCount} // Hosted by Drowns#4865"
                      });

                      if (disband)
                      {
                          embed.AddField("Ban Appeal Server", "[Pokemon Automation](https://discord.gg/pokemonautomation)", true);
                          embed.AddField("Appeal Here", "[#tera-raid-bans](https://discord.com/channels/695809740428673034/1050667958562738197)", true);
                      }

                      if (!disband && names is null && Settings.CodeInInfo)
                      {
                          if (Settings.CodeIfSplitHidden && rcode.Length == 6)
                          {
                              embed.AddField("**Waiting in lobby!**", $"Raid code: ||{rcode.Substring(0, rcode.Length / 2)}||᲼+᲼||{rcode.Substring(rcode.Length / 2)}||");
                          }
                          else
                          {
                              embed.AddField("**Waiting in lobby!**", $"Raid code: {rcode}");
                          }
                      }

                      if (!disband && !starting)
                      {
                          embed.AddField("IVs:", $"{Settings.RaidSpeciesIVs}", true);
                          embed.AddField("Nature:", $"{Settings.RaidSpeciesNature}", true);
                          embed.AddField("Ability:", $"{Settings.RaidSpeciesAbility}", true);
                      }
                */
                if (!disband && names is not null && starting)
                {
                    var players = string.Empty;
                    if (names.Count == 0)
                        players = "Though our party did not make it :(";
                    else
                    {
                        int i = 2;
                        names.ForEach(x =>
                        {
                            players += $"Player {i} - **{x}**\n";
                            i++;
                        });
                    }

                 //   embed.AddField($"**Players:**", players);
                }

               // EmbedQueue.Enqueue((bytes, embed));
            }
        }

        // From PokeTradeBotSV, modified.
        private async Task<bool> ConnectToOnline(PokeTradeHubConfig config, CancellationToken token)
        {
            if (await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
                return true;

            await Click(X, 3_000, token).ConfigureAwait(false);
            await Click(L, 5_000 + config.Timings.ExtraTimeConnectOnline, token).ConfigureAwait(false);

            // Try one more time.
            if (!await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
            {
                Log("Failed to connect the first time, trying again...");
                await RecoverToOverworld(token).ConfigureAwait(false);
                await Click(X, 3_000, token).ConfigureAwait(false);
                await Click(L, 5_000 + config.Timings.ExtraTimeConnectOnline, token).ConfigureAwait(false);
            }

            var wait = 0;
            while (!await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
            {
                await Task.Delay(0_500, token).ConfigureAwait(false);
                if (++wait > 30) // More than 15 seconds without a connection.
                    return false;
            }

            // There are several seconds after connection is established before we can dismiss the menu.
            await Task.Delay(3_000 + config.Timings.ExtraTimeConnectOnline, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);
            return true;
        }

        // From PokeTradeBotSV.
        private async Task<bool> RecoverToOverworld(CancellationToken token)
        {
            if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                return true;

            Log("Attempting to recover to overworld.");
            var attempts = 0;
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                attempts++;
                if (attempts >= 30)
                    break;

                await Click(B, 1_300, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;

                await Click(B, 2_000, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;

                await Click(A, 1_300, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;
            }

            // We didn't make it for some reason.
            if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                Log("Failed to recover to overworld, rebooting the game.");
                await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
            }
            await Task.Delay(1_000, token).ConfigureAwait(false);
            return true;
        }
        private static string GetDodoURL()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bot 69804372.Njk4MDQzNzI.77-9OW_vv70.qvJQfqTiyAXPJlZx1THOL8hp2H3MjISyFpficc6OOOM");
                MultipartFormDataContent contentFormData = new MultipartFormDataContent();
                string path = @"D:\Desktop\a.jpg";
                contentFormData.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(path)), "file", "b.jpg");
                var requestUri = @"https://botopen.imdodo.com/api/v2/resource/picture/upload";
                var result = client.PostAsync(requestUri, contentFormData).Result.Content.ReadAsStringAsync().Result;
                var a = result.Split("https");
                var b = a[1].Split("jpg");
                var c = "https" + b[0] + "jpg";
                return c;
            }

        }

    }
}
