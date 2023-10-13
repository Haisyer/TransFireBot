using System.Linq;
using PKHeX.Core;
using PKHeX.Core.Searching;
using SysBot.Base;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsSV;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace SysBot.Pokemon
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class PokeTradeBotSV : PokeRoutineExecutor9SV, ICountBot
    {
        private readonly PokeTradeHub<PK9> Hub;
        private readonly TradeSettings TradeSettings;
        public readonly TradeAbuseSettings AbuseSettings;

        public ICountSettings Counts => TradeSettings;

        /// <summary>
        /// Folder to dump received trade data to.
        /// </summary>
        /// <remarks>If null, will skip dumping.</remarks>
        private readonly IDumper DumpSetting;

        private readonly string TradeF;

        /// <summary>
        /// Synchronized start for multiple bots.
        /// </summary>
        public bool ShouldWaitAtBarrier { get; private set; }

        /// <summary>
        /// Tracks failed synchronized starts to attempt to re-sync.
        /// </summary>
        public int FailedBarrier { get; private set; }

        public PokeTradeBotSV(PokeTradeHub<PK9> hub, PokeBotState cfg) : base(cfg)
        {
            Hub = hub;
            TradeSettings = hub.Config.Trade;
            AbuseSettings = hub.Config.TradeAbuse;
            DumpSetting = hub.Config.Folder;
            TradeF = hub.Config.Folder.TradeFolder;
            lastOffered = new byte[8];

        }

        // Cached offsets that stay the same per session.
        private ulong BoxStartOffset;
        private ulong OverworldOffset;
        private ulong PortalOffset;
        private ulong ConnectedOffset;
        private ulong TradePartnerNIDOffset;
        private ulong TradePartnerOfferedOffset;

        // Store the current save's OT and TID/SID for comparison.
        private string OT = string.Empty;
        private uint DisplaySID;
        private uint DisplayTID;

        // Stores whether we returned all the way to the overworld, which repositions the cursor.
        private bool StartFromOverworld = true;
        // Stores whether the last trade was Distribution with fixed code, in which case we don't need to re-enter the code.
        private bool LastTradeDistributionFixed;
        // Track the last Pokémon we were offered since it persists between trades.
        private byte[] lastOffered;
        public override async Task MainLoop(CancellationToken token)
        {
            try
            {
                await InitializeHardware(Hub.Config.Trade, token).ConfigureAwait(false);

                Log("正在识别主机的训练家数据...");
                var sav = await IdentifyTrainer(token).ConfigureAwait(false);
                OT = sav.OT;
                DisplaySID = sav.DisplaySID;
                DisplayTID = sav.DisplayTID;
                RecentTrainerCache.SetRecentTrainer(sav);
                await InitializeSessionOffsets(token).ConfigureAwait(false);

                // Force the bot to go through all the motions again on its first pass.
                StartFromOverworld = true;
                LastTradeDistributionFixed = false;

                Log($"开始 {nameof(PokeTradeBotSV)} 主循环.");
                await InnerLoop(sav, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            Log($"结束{nameof(PokeTradeBotSV)}循环.");
            await HardStop().ConfigureAwait(false);
        }

        public override async Task HardStop()
        {
            UpdateBarrier(false);
            await CleanExit(CancellationToken.None).ConfigureAwait(false);
        }

        private async Task InnerLoop(SAV9SV sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Config.IterateNextRoutine();
                var task = Config.CurrentRoutineType switch
                {
                    PokeRoutineType.Idle => DoNothing(token),
                    _ => DoTrades(sav, token),
                };
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (SocketException e)
                {
                    if (e.StackTrace != null)
                        Connection.LogError(e.StackTrace);
                    var attempts = Hub.Config.Timings.ReconnectAttempts;
                    var delay = Hub.Config.Timings.ExtraReconnectDelay;
                    var protocol = Config.Connection.Protocol;
                    if (!await TryReconnect(attempts, delay, protocol, token).ConfigureAwait(false))
                        return;
                }
            }
        }

        private async Task DoNothing(CancellationToken token)
        {
            Log("没有分配任务。正在等待新的任务分配...");
            while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.Idle)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }

        private async Task DoTrades(SAV9SV sav, CancellationToken token)
        {
            var type = Config.CurrentRoutineType;
            int waitCounter = 0;
            await SetCurrentBox(0, token).ConfigureAwait(false);
            while (!token.IsCancellationRequested && Config.NextRoutineType == type)
            {
                var (detail, priority) = GetTradeData(type);
                if (detail is null)
                {
                    await WaitForQueueStep(waitCounter++, token).ConfigureAwait(false);
                    continue;
                }
                waitCounter = 0;

                detail.IsProcessing = true;
                string tradetype = $" ({detail.Type})";
                Log($"开始进行下一个 {type}{tradetype} 机器人交易。正在获取用户数据...");
                Hub.Config.Stream.StartTrade(this, detail, Hub);
                Hub.Queues.StartTrade(this, detail);

                await PerformTrade(sav, detail, type, priority, token).ConfigureAwait(false);
            }
        }

        private async Task WaitForQueueStep(int waitCounter, CancellationToken token)
        {
            if (waitCounter == 0)
            {
                // Updates the assets.
                Hub.Config.Stream.IdleAssets(this);
                Log("没人使用，正在等待新用户...");
            }

            await Task.Delay(1_000, token).ConfigureAwait(false);
        }

        protected virtual (PokeTradeDetail<PK9>? detail, uint priority) GetTradeData(PokeRoutineType type)
        {
            if (Hub.Queues.TryDequeue(type, out var detail, out var priority))
                return (detail, priority);
            if (Hub.Queues.TryDequeueLedy(out detail))
                return (detail, PokeTradePriorities.TierFree);
            return (null, PokeTradePriorities.TierFree);
        }

        private async Task PerformTrade(SAV9SV sav, PokeTradeDetail<PK9> detail, PokeRoutineType type, uint priority, CancellationToken token)
        {
            PokeTradeResult result;
            try
            {
                result = await PerformLinkCodeTrade(sav, detail, token).ConfigureAwait(false);
                if (result == PokeTradeResult.Success)
                    return;
            }
            catch (SocketException socket)
            {
                Log(socket.Message);
                result = PokeTradeResult.ExceptionConnection;
                HandleAbortedTrade(detail, type, priority, result);
                throw; // let this interrupt the trade loop. re-entering the trade loop will recheck the connection.
            }
            catch (Exception e)
            {
                Log(e.Message);
                result = PokeTradeResult.ExceptionInternal;
            }

            HandleAbortedTrade(detail, type, priority, result);
        }

        private void HandleAbortedTrade(PokeTradeDetail<PK9> detail, PokeRoutineType type, uint priority, PokeTradeResult result)
        {
            detail.IsProcessing = false;
            if (result.ShouldAttemptRetry() && detail.Type != PokeTradeType.Random && !detail.IsRetry)
            {
                detail.IsRetry = true;
                Hub.Queues.Enqueue(type, detail, Math.Min(priority, PokeTradePriorities.Tier2));
                detail.SendNotification(this, "哦!发生了一些意外。我会安排您再试一次。");
            }
            else
            {
                detail.SendNotification(this, $"哦!发生了一些意外。取消交易: {result}.");
                detail.TradeCanceled(this, result);
            }
        }

        private async Task<PokeTradeResult> PerformLinkCodeTrade(SAV9SV sav, PokeTradeDetail<PK9> poke, CancellationToken token)
        {
            // Update Barrier Settings
            UpdateBarrier(poke.IsSynchronized);
            poke.TradeInitialize(this);
            Hub.Config.Stream.EndEnterCode(this);

            // StartFromOverworld can be true on first pass or if something went wrong last trade.
            if (StartFromOverworld && !await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await RecoverToOverworld(token).ConfigureAwait(false);

            // Handles getting into the portal. Will retry this until successful.
            // if we're not starting from overworld, then ensure we're online before opening link trade -- will break the bot otherwise.
            // If we're starting from overworld, then ensure we're online before opening the portal.
            if (!StartFromOverworld && !await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
            {
                await RecoverToOverworld(token).ConfigureAwait(false);
                if (!await ConnectAndEnterPortal(token).ConfigureAwait(false))
                {
                    await RecoverToOverworld(token).ConfigureAwait(false);
                    return PokeTradeResult.RecoverStart;
                }
            }
            else if (StartFromOverworld && !await ConnectAndEnterPortal(token).ConfigureAwait(false))
            {
                await RecoverToOverworld(token).ConfigureAwait(false);
                return PokeTradeResult.RecoverStart;
            }

            var toSend = poke.TradeData;
            LogUtil.LogInfo($"尝试写入盒子,宝可梦种类id:{toSend.Species}", nameof(PokeTradeBotSV));
            if (toSend.Species != 0)
            {
                await SetBoxPokemonAbsolute(BoxStartOffset, toSend, token, sav).ConfigureAwait(false);
                LogUtil.LogInfo($"已经写入盒子,宝可梦种类id:{toSend.Species}", nameof(PokeTradeBotSV));
            }
            var tempReadBoxPokemon = await ReadBoxPokemon(1, 1, token).ConfigureAwait(false);
            if (toSend.Species != tempReadBoxPokemon.Species)
            {
                LogUtil.LogInfo($"！！！！计划写入的宝可梦种类id:{toSend.Species},读取出来的:{tempReadBoxPokemon.Species}", nameof(PokeTradeBotSV));
            }

            // Assumes we're freshly in the Portal and the cursor is over Link Trade.
            Log("正在选择连接交换...");

            await Click(A, 1_500, token).ConfigureAwait(false);
            // Make sure we clear any Link Codes if we're not in Distribution with fixed code, and it wasn't entered last round.
            if (poke.Type != PokeTradeType.Random || !LastTradeDistributionFixed)
            {
                await Click(X, 1_000, token).ConfigureAwait(false);
                await Click(PLUS, 1_000, token).ConfigureAwait(false);

                // Loading code entry.
                if (poke.Type != PokeTradeType.Random)
                    Hub.Config.Stream.StartEnterCode(this);
                await Task.Delay(Hub.Config.Timings.ExtraTimeOpenCodeEntry, token).ConfigureAwait(false);

                var code = poke.Code;
                Log($"正在输入连接交换密码: {code:0000 0000}");
                await EnterLinkCode(code, Hub.Config, token).ConfigureAwait(false);
                if (Hub.Config.Dodo.DodoScreenshot)
                {
                    var bytes = await SwitchConnection.Screengrab(token).ConfigureAwait(false) ?? Array.Empty<byte>();
                    var result = GetDodoURL(bytes);
                    poke.SendNotification(this, toSend, result);
                }
                await Click(PLUS, 3_000, token).ConfigureAwait(false);
                StartFromOverworld = false;
            }

            LastTradeDistributionFixed = poke.Type == PokeTradeType.Random && !Hub.Config.Distribution.RandomCode;

            // Search for a trade partner for a Link Trade.
            await Click(A, 0_500, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);

            // Clear it so we can detect it loading.
            await ClearTradePartnerNID(TradePartnerNIDOffset, token).ConfigureAwait(false);

            // Wait for Barrier to trigger all bots simultaneously.
            WaitAtBarrierIfApplicable(token);
            await Click(A, 1_000, token).ConfigureAwait(false);

            poke.TradeSearching(this);

            // Wait for a Trainer...
            var partnerFound = await WaitForTradePartner(token).ConfigureAwait(false);

            if (token.IsCancellationRequested)
            {
                StartFromOverworld = true;
                LastTradeDistributionFixed = false;
                await ExitTradeToPortal(false, token).ConfigureAwait(false);
                return PokeTradeResult.RoutineCancel;
            }
            if (!partnerFound)
            {
                if (!await RecoverToPortal(token).ConfigureAwait(false))
                {
                    Log("无法返回到宝可站.");
                    await RecoverToOverworld(token).ConfigureAwait(false);
                }
                return PokeTradeResult.NoTrainerFound;
            }

            Hub.Config.Stream.EndEnterCode(this);

            // Wait until we get into the box.
            var cnt = 0;
            while (!await IsInBox(PortalOffset, token).ConfigureAwait(false))
            {
                await Task.Delay(0_500, token).ConfigureAwait(false);
                if (++cnt > 20) // Didn't make it in after 10 seconds.
                {
                    await Click(A, 1_000, token).ConfigureAwait(false); // Ensures we dismiss a popup.
                    if (!await RecoverToPortal(token).ConfigureAwait(false))
                    {
                        Log("无法返回到宝可站.");
                        await RecoverToOverworld(token).ConfigureAwait(false);
                    }
                    return PokeTradeResult.RecoverOpenBox;
                }
            }
            await Task.Delay(3_000 + Hub.Config.Timings.ExtraTimeOpenBox, token).ConfigureAwait(false);

            var tradePartnerFullInfo = await GetTradePartnerFullInfo(token).ConfigureAwait(false);
            var tradePartner = new TradePartnerSV(tradePartnerFullInfo);
            var trainerNID = await GetTradePartnerNID(TradePartnerNIDOffset, token).ConfigureAwait(false);
            var myNID = await GetMyNID(Offsets.MyNIDPointer, token).ConfigureAwait(false);
            RecordUtil<PokeTradeBotSWSH>.Record($"找到连接交换对象\tNID:{trainerNID:X16}\tOT_Name:{tradePartner.TrainerName}\t平台昵称:{poke.Trainer.TrainerName}\t平台ID:{poke.Trainer.ID}\t序列号:{poke.ID}\tEC:{toSend.EncryptionConstant:X8}");
            Log($"找到连接交换对象: {tradePartner.TrainerName}-TID:{tradePartner.TID7}-SID:{tradePartner.SID7}(任天堂网络ID: {trainerNID})");

            var partnerCheck = await CheckPartnerReputation(this, poke, trainerNID, tradePartner.TrainerName, AbuseSettings, token, myNID);
            if (partnerCheck != PokeTradeResult.Success)
            {
                await Click(A, 1_000, token).ConfigureAwait(false); // Ensures we dismiss a popup.
                await ExitTradeToPortal(false, token).ConfigureAwait(false);
                return partnerCheck;
            }
            // Hard check to verify that the offset changed from the last thing offered from the previous trade.
            // This is because box opening times can vary per person, the offset persists between trades, and can also change offset between trades.
            var tradeOffered = await ReadUntilChanged(TradePartnerOfferedOffset, lastOffered, 10_000, 0_500, false, true, token).ConfigureAwait(false);
            if (!tradeOffered)
                return PokeTradeResult.TrainerTooSlow;
            poke.SendNotification(this, $"Found Trading Partner:{tradePartner.TrainerName}. TID: {tradePartner.TID7} SID: {tradePartner.SID7} Waiting for a Pokémon...");//勿动
            if (poke.Type == PokeTradeType.Dump)
            {
                var result = await ProcessDumpTradeAsync(poke, token).ConfigureAwait(false);
                await ExitTradeToPortal(false, token).ConfigureAwait(false);
                return result;
            }
            int waittime = 25_000;
            List<PK9> ls = new List<PK9>();
            if (poke.Type == PokeTradeType.MutiTrade || poke.DeletFile)
            {
                waittime = 375_000;
                string directory = Path.Combine(TradeF, poke.Path);
                string[] fileEntries = Directory.GetFiles(directory);
                Array.Sort(fileEntries);
                Log($"读取到的文件数量:{fileEntries.Length}");
                foreach (string fileName in fileEntries)
                {
                    var data = File.ReadAllBytes(fileName);
                   // LogUtil.LogInfo($"读取到文件:{fileName}",nameof(PokeTradeBotSV));
                    var pkt = EntityFormat.GetFromBytes(data);
                    if (pkt != null)
                    {
                        pkt.RefreshChecksum();
                        if (EntityConverter.ConvertToType(pkt, typeof(PK9), out _) is PK9 pk2)
                            ls.Add(pk2);
                    }
                }
                if (Directory.Exists(directory) && poke.DeletFile)
                {
                    foreach (var item in Directory.GetFiles(directory))
                    {
                        File.Delete(item);
                    }
                    Directory.Delete(directory);
                }

            }
            else
            {
                ls.Add(poke.TradeData);
            }
            PK9 offered = toSend;
            int counting = 0;
            foreach (var send in ls)
            {
                counting++;
                toSend = send;
                await SetBoxPokemonAbsolute(BoxStartOffset, toSend, token, sav).ConfigureAwait(false);//先写一次箱子
                //if (ls.Count > 1)
                //{
                //  poke.SendNotification(this, $"批量:等待交换第{counting}个宝可梦{ShowdownTranslator<PK9>.GameStringsZh.Species[toSend.Species]}");
                //  LogUtil.LogInfo($"批量:等待交换第{counting}个宝可梦{ShowdownTranslator<PK9>.GameStringsZh.Species[toSend.Species]}",nameof(PokeTradeBotSV));
                //}

                //软件外置添加模板ban人，后加
                var entry = AbuseSettings.BanFile.List.Find(z => z.Name.Equals(toSend.OT_Name));
                Log($"Current OT is: " + toSend.OT_Name);
                if (entry != null)
                {
                    Log($"该模板ID被禁止");
                    poke.SendNotification(this, $"该模板ID被禁止\n{entry.Comment}");
                    AbuseSettings.BannedIDs.AddIfNew(new[] { GetReference(tradePartner.TrainerName, trainerNID, "使用禁止模板") });
                    await Click(A, 1_000, token).ConfigureAwait(false); // Ensures we dismiss a popup.
                    await ExitTradeToPortal(false, token).ConfigureAwait(false);
                    return PokeTradeResult.SuspiciousActivity;
                }
                //内置ban人，数组内加名字
                string[] wretchName = { "大队长", "DDZ", "Ddz", "DDz", "dDz", "dDZ", "ddz", "ddZ", "叫我大队长", "我是大队长", "忘世麒麟", "叫我大隊長", "我是大隊長", "大隊長" };
                Log($"Current OT is: " + toSend.OT_Name);

                foreach (var itemName in wretchName)
                {
                    if (string.Equals(toSend.OT_Name, itemName))
                    {
                        Log($"有狗");
                        poke.SendNotification(this, itemName);
                        AbuseSettings.BannedIDs.AddIfNew(new[] { GetReference(tradePartner.TrainerName, trainerNID, "大队长与狗不能交换") });
                        await Click(A, 1_000, token).ConfigureAwait(false); // Ensures we dismiss a popup.
                        await ExitTradeToPortal(false, token).ConfigureAwait(false);
                        return PokeTradeResult.SuspiciousActivity;
                    }
                }


                if (Hub.Config.Legality.UseTradePartnerInfo)
                {
                    await SetBoxPkmWithSwappedIDDetailsSV(toSend, tradePartnerFullInfo, sav, poke.MODID, token);
                }
                Log("正在等待对方提供一只宝可梦...");
                // Wait for user input...
                offered = await ReadUntilPresentMutiTrade(TradePartnerOfferedOffset, offered, counting, waittime, 1_000, BoxFormatSlotSize, token).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("ReadUntilPresentMutiTrade方法返回结果为null.");
                var oldEC = await SwitchConnection.ReadBytesAbsoluteAsync(TradePartnerOfferedOffset, 8, token).ConfigureAwait(false);
                if (offered == null || offered.Species < 1 || !offered.ChecksumValid)
                {
                    Log("交易结束，因为没有提供有效的宝可梦.");
                    await ExitTradeToPortal(false, token).ConfigureAwait(false);
                    return PokeTradeResult.TrainerTooSlow;
                }

                PokeTradeResult update;
                var trainer = new PartnerDataHolder(0, tradePartner.TrainerName, tradePartner.TID7);
                (toSend, update) = await GetEntityToSend(sav, poke, offered, oldEC, toSend, trainer, token).ConfigureAwait(false);
                if (update != PokeTradeResult.Success)
                {
                    await ExitTradeToPortal(false, token).ConfigureAwait(false);
                    return update;
                }

                Log("正在确认交易...");
                var tradeResult = await ConfirmAndStartTrading(poke, token).ConfigureAwait(false);
                if (tradeResult != PokeTradeResult.Success)
                {
                    await ExitTradeToPortal(false, token).ConfigureAwait(false);
                    return tradeResult;
                }
                if (ls.Count > 1)
                {
                    poke.SendNotification(this, $"批量:第{counting}只宝可梦{ShowdownTranslator<PK9>.GameStringsZh.Species[toSend.Species]}，交换完成");
                    LogUtil.LogInfo($"批量:等待交换第{counting}个宝可梦{ShowdownTranslator<PK9>.GameStringsZh.Species[toSend.Species]}", nameof(PokeTradeBotSV));
                }
                if (token.IsCancellationRequested)
                {
                    StartFromOverworld = true;
                    LastTradeDistributionFixed = false;
                    await ExitTradeToPortal(false, token).ConfigureAwait(false);
                    return PokeTradeResult.RoutineCancel;
                }
            }
            // Trade was Successful!
            var received = await ReadPokemon(BoxStartOffset, BoxFormatSlotSize, token).ConfigureAwait(false);
            // Pokémon in b1s1 is same as the one they were supposed to receive (was never sent).
            if (SearchUtil.HashByDetails(received) == SearchUtil.HashByDetails(toSend) && received.Checksum == toSend.Checksum)
            {
                Log("{没有完成交易.");
                await ExitTradeToPortal(false, token).ConfigureAwait(false);
                return PokeTradeResult.TrainerTooSlow;
            }

            // As long as we got rid of our inject in b1s1, assume the trade went through.
            Log("用户完成交易.");
            poke.TradeFinished(this, received);

            // Only log if we completed the trade.
            UpdateCountsAndExport(poke, received, toSend);
            // Log for Trade Abuse tracking.
            LogSuccessfulTrades(poke, trainerNID, tradePartner.TrainerName);
            // Sometimes they offered another mon, so store that immediately upon leaving Union Room.
            lastOffered = await SwitchConnection.ReadBytesAbsoluteAsync(TradePartnerOfferedOffset, 8, token).ConfigureAwait(false);
            await ExitTradeToPortal(false, token).ConfigureAwait(false);
            return PokeTradeResult.Success;
        }

        private void UpdateCountsAndExport(PokeTradeDetail<PK9> poke, PK9 received, PK9 toSend)
        {
            var counts = TradeSettings;
            if (poke.Type == PokeTradeType.Random)
                counts.AddCompletedDistribution();
            else if (poke.Type == PokeTradeType.Clone)
                counts.AddCompletedClones();
            else
                counts.AddCompletedTrade();

            if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
            {
                var subfolder = poke.Type.ToString().ToLower();
                DumpPokemon(DumpSetting.DumpFolder, subfolder, received); // received by bot
                if (poke.Type is PokeTradeType.Specific or PokeTradeType.Clone)
                    DumpPokemon(DumpSetting.DumpFolder, "traded", toSend); // sent to partner
            }
        }

        private async Task<PokeTradeResult> ConfirmAndStartTrading(PokeTradeDetail<PK9> detail, CancellationToken token)
        {
            // We'll keep watching B1S1 for a change to indicate a trade started -> should try quitting at that point.
            var oldEC = await SwitchConnection.ReadBytesAbsoluteAsync(BoxStartOffset, 8, token).ConfigureAwait(false);

            await Click(A, 3_000, token).ConfigureAwait(false);
            for (int i = 0; i < Hub.Config.Trade.MaxTradeConfirmTime; i++)
            {
                if (await IsUserBeingShifty(detail, token).ConfigureAwait(false))
                    return PokeTradeResult.SuspiciousActivity;

                // We can fall out of the box if the user offers, then quits.
                if (!await IsInBox(PortalOffset, token).ConfigureAwait(false))
                    return PokeTradeResult.TrainerLeft;

                await Click(A, 1_000, token).ConfigureAwait(false);

                // EC is detectable at the start of the animation.
                var newEC = await SwitchConnection.ReadBytesAbsoluteAsync(BoxStartOffset, 8, token).ConfigureAwait(false);
                if (!newEC.SequenceEqual(oldEC))
                {
                    await Task.Delay(25_000, token).ConfigureAwait(false);
                    return PokeTradeResult.Success;
                }
            }
            // If we don't detect a B1S1 change, the trade didn't go through in that time.
            return PokeTradeResult.TrainerTooSlow;
        }

        // Upon connecting, their Nintendo ID will instantly update.
        protected virtual async Task<bool> WaitForTradePartner(CancellationToken token)
        {
            Log("正在等待训练家...");
            int ctr = (Hub.Config.Trade.TradeWaitTime * 1_000) - 2_000;
            await Task.Delay(2_000, token).ConfigureAwait(false);
            while (ctr > 0)
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                ctr -= 1_000;
                var newNID = await GetTradePartnerNID(TradePartnerNIDOffset, token).ConfigureAwait(false);
                if (newNID != 0)
                {
                    TradePartnerOfferedOffset = await SwitchConnection.PointerAll(Offsets.LinkTradePartnerPokemonPointer, token).ConfigureAwait(false);
                    return true;
                }

                // Fully load into the box.
                await Task.Delay(1_000, token).ConfigureAwait(false);
            }
            return false;
        }

        // If we can't manually recover to overworld, reset the game.
        // Try to avoid pressing A which can put us back in the portal with the long load time.
        private async Task<bool> RecoverToOverworld(CancellationToken token)
        {
            if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                return true;

            Log("尝试关闭所有界面。");
            var attempts = 0;
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                attempts++;
                if (attempts >= 30)
                    break;

                await Click(B, 1_000, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;

                await Click(B, 1_000, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;

                if (await IsInBox(PortalOffset, token).ConfigureAwait(false))
                    await Click(A, 1_000, token).ConfigureAwait(false);
            }

            // We didn't make it for some reason.
            if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                Log("无法关闭所有界面，重新启动游戏。");
                await RestartGameSV(token).ConfigureAwait(false);
            }
            await Task.Delay(1_000, token).ConfigureAwait(false);

            // Force the bot to go through all the motions again on its first pass.
            StartFromOverworld = true;
            LastTradeDistributionFixed = false;
            return true;
        }

        // If we didn't find a trainer, we're still in the portal but there can be 
        // different numbers of pop-ups we have to dismiss to get back to when we can trade.
        // Rather than resetting to overworld, try to reset out of portal and immediately go back in.
        private async Task<bool> RecoverToPortal(CancellationToken token)
        {
            Log("重新定向到宝可站。");
            var attempts = 0;
            while (await IsInPokePortal(PortalOffset, token).ConfigureAwait(false))
            {
                await Click(B, 2_500, token).ConfigureAwait(false);
                if (++attempts >= 30)
                {
                    Log("无法恢复到宝可站.");
                    return false;
                }
            }

            // Should be in the X menu hovered over Poké Portal.
            await Click(A, 1_000, token).ConfigureAwait(false);

            return await SetUpPortalCursor(token).ConfigureAwait(false);
        }

        // Should be used from the overworld. Opens X menu, attempts to connect online, and enters the Portal.
        // The cursor should be positioned over Link Trade.
        private async Task<bool> ConnectAndEnterPortal(CancellationToken token)
        {
            if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await RecoverToOverworld(token).ConfigureAwait(false);

            Log("打开宝可站");

            // Open the X Menu.
            await Click(X, 1_000, token).ConfigureAwait(false);

            // Connect online if not already.
            // Handle the news popping up.
            if (await SwitchConnection.IsProgramRunning(LibAppletWeID, token).ConfigureAwait(false))
            {
                Log("检测到新闻，将在加载后关闭!");
                await Task.Delay(5_000, token).ConfigureAwait(false);
                await Click(B, 2_000, token).ConfigureAwait(false);
            }

            // Scroll to the bottom of the Main Menu so we don't need to care if Picnic is unlocked.
            await Click(DRIGHT, 0_300, token).ConfigureAwait(false);
            await PressAndHold(DDOWN, 1_000, 1_000, token).ConfigureAwait(false);
            await Click(DUP, 0_200, token).ConfigureAwait(false);
            await Click(DUP, 0_200, token).ConfigureAwait(false);
            await Click(DUP, 0_200, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);

            return await SetUpPortalCursor(token).ConfigureAwait(false);
        }

        // Waits for the Portal to load (slow) and then moves the cursor down to link trade.
        private async Task<bool> SetUpPortalCursor(CancellationToken token)
        {
            // Wait for the portal to load.
            var attempts = 0;
            while (!await IsInPokePortal(PortalOffset, token).ConfigureAwait(false))
            {
                await Task.Delay(0_500, token).ConfigureAwait(false);
                if (++attempts > 20)
                {
                    Log("加载宝可站失败.");
                    return false;
                }
            }
            await Task.Delay(2_000 + Hub.Config.Timings.ExtraTimeLoadPortal, token).ConfigureAwait(false);
            // Connect online if not already.
            if (!await ConnectToOnline(Hub.Config, token).ConfigureAwait(false))
            {
                Log("Failed to connect to online.");
                return false; // Failed, either due to connection or softban.
            }
            // Handle the news popping up.
            if (await SwitchConnection.IsProgramRunning(LibAppletWeID, token).ConfigureAwait(false))
            {
                Log("检测到的新闻，将在加载后关闭!");
                await Task.Delay(5_000, token).ConfigureAwait(false);
                await Click(B, 2_000 + Hub.Config.Timings.ExtraTimeLoadPortal, token).ConfigureAwait(false);
            }

            Log("正在移动界面的光标...");
            // Move down to Link Trade.
            await Click(DDOWN, 0_300, token).ConfigureAwait(false);
            await Click(DDOWN, 0_300, token).ConfigureAwait(false);
            return true;
        }

        // Connects online if not already. Assumes the user to be in the X menu to avoid a news screen.
        private async Task<bool> ConnectToOnline(PokeTradeHubConfig config, CancellationToken token)
        {
            if (await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
                return true;

            await Click(L, 5_000, token).ConfigureAwait(false);

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

        private async Task ExitTradeToPortal(bool unexpected, CancellationToken token)
        {
            await Task.Delay(1_000, token).ConfigureAwait(false);
            if (await IsInPokePortal(PortalOffset, token).ConfigureAwait(false))
                return;

            if (unexpected)
                Log("异常行为，正在返回到宝可入口站。");

            // Ensure we're not in the box first.
            // Takes a long time for the Portal to load up, so once we exit the box, wait 5 seconds.
            Log("正在离开箱子...");
            var attempts = 0;
            while (await IsInBox(PortalOffset, token).ConfigureAwait(false))
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                if (!await IsInBox(PortalOffset, token).ConfigureAwait(false))
                {
                    await Task.Delay(1_000, token).ConfigureAwait(false);
                    break;
                }

                await Click(A, 1_000, token).ConfigureAwait(false);
                if (!await IsInBox(PortalOffset, token).ConfigureAwait(false))
                {
                    await Task.Delay(1_000, token).ConfigureAwait(false);
                    break;
                }

                await Click(B, 1_000, token).ConfigureAwait(false);
                if (!await IsInBox(PortalOffset, token).ConfigureAwait(false))
                {
                    await Task.Delay(1_000, token).ConfigureAwait(false);
                    break;
                }

                // Didn't make it out of the box for some reason.
                if (++attempts > 20)
                {
                    Log("退出盒子失败，重新启动游戏。");
                    if (!await RecoverToOverworld(token).ConfigureAwait(false))
                        await RestartGameSV(token).ConfigureAwait(false);
                    await ConnectAndEnterPortal(token).ConfigureAwait(false);
                    return;
                }
            }

            // Wait for the portal to load.
            Log("正在等待宝可站加载...");
            attempts = 0;
            while (!await IsInPokePortal(PortalOffset, token).ConfigureAwait(false))
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                if (await IsInPokePortal(PortalOffset, token).ConfigureAwait(false))
                    break;

                // Didn't make it into the portal for some reason.
                if (++attempts > 40)
                {
                    Log("加载宝可站失败，重新启动游戏。");
                    if (!await RecoverToOverworld(token).ConfigureAwait(false))
                        await RestartGameSV(token).ConfigureAwait(false);
                    await ConnectAndEnterPortal(token).ConfigureAwait(false);
                    return;
                }
            }
        }

        // These don't change per session and we access them frequently, so set these each time we start.
        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("正在缓存会话偏移量...");
            BoxStartOffset = await SwitchConnection.PointerAll(Offsets.BoxStartPokemonPointer, token).ConfigureAwait(false);
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
            PortalOffset = await SwitchConnection.PointerAll(Offsets.PortalBoxStatusPointer, token).ConfigureAwait(false);
            ConnectedOffset = await SwitchConnection.PointerAll(Offsets.IsConnectedPointer, token).ConfigureAwait(false);
            TradePartnerNIDOffset = await SwitchConnection.PointerAll(Offsets.LinkTradePartnerNIDPointer, token).ConfigureAwait(false);
        }

        // todo: future
        protected virtual async Task<bool> IsUserBeingShifty(PokeTradeDetail<PK9> detail, CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            return false;
        }

        private async Task RestartGameSV(CancellationToken token)
        {
            await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
            await InitializeSessionOffsets(token).ConfigureAwait(false);
        }

        private async Task<PokeTradeResult> ProcessDumpTradeAsync(PokeTradeDetail<PK9> detail, CancellationToken token)
        {
            int ctr = 0;
            var time = TimeSpan.FromSeconds(Hub.Config.Trade.MaxDumpTradeTime);
            var start = DateTime.Now;

            var pkprev = new PK9();
            var bctr = 0;
            var n = 1;
            Log("正在检测...");
            while (ctr < Hub.Config.Trade.MaxDumpsPerTrade && DateTime.Now - start < time)
            {
                if (!await IsInBox(PortalOffset, token).ConfigureAwait(false))
                    break;
                if (bctr++ % 3 == 0)
                    await Click(B, 0_100, token).ConfigureAwait(false);

                // Wait for user input... Needs to be different from the previously offered Pokémon.
                var pk = await ReadUntilPresent(TradePartnerOfferedOffset, 3_000, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                if (pk == null || pk.Species < 1 || !pk.ChecksumValid || SearchUtil.HashByDetails(pk) == SearchUtil.HashByDetails(pkprev))
                    continue;

                // Save the new Pokémon for comparison next round.
                pkprev = pk;

                // Send results from separate thread; the bot doesn't need to wait for things to be calculated.
                if (DumpSetting.Dump)
                {
                    var subfolder = detail.Type.ToString().ToLower();
                    DumpPokemon(DumpSetting.DumpFolder, subfolder, pk); // received
                }

                //    var la = new LegalityAnalysis(pk);
                //    var verbose = $"```{la.Report(true)}```";
                //    Log($"显示的宝可梦是: {(la.Valid ? "Valid" : "Invalid")}.");

                ctr++;
                var msg = $"检测第{n}只";
                detail.SendNotification(this, pk, msg);
                n++;
                TradeExtensions<PK9>.EggLogs(pk);
            }

            Log($"在处理 {ctr} 宝可梦完成后结束转储循环。");
            if (ctr == 0)
                return PokeTradeResult.TrainerTooSlow;

            TradeSettings.AddCompletedDumps();
            detail.Notifier.SendNotification(this, detail, $"Dumped {ctr} Pokémon.");
            detail.Notifier.TradeFinished(this, detail, detail.TradeData); // blank PK9
            return PokeTradeResult.Success;
        }

        private async Task<TradePartnerSV> GetTradePartnerInfo(CancellationToken token)
        {
            return new TradePartnerSV(await GetTradePartnerFullInfo(token));
        }

        private async Task<TradeMyStatus> GetTradePartnerFullInfo(CancellationToken token)
        {
            // We're able to see both users' MyStatus, but one of them will be ourselves.
            var trader_info = await GetTradePartnerMyStatus(Offsets.Trader1MyStatusPointer, token).ConfigureAwait(false);
            if (trader_info.OT == OT && trader_info.DisplaySID == DisplaySID && trader_info.DisplayTID == DisplayTID) // This one matches ourselves.
                trader_info = await GetTradePartnerMyStatus(Offsets.Trader2MyStatusPointer, token).ConfigureAwait(false);
            return trader_info;
        }

        protected virtual async Task<(PK9 toSend, PokeTradeResult check)> GetEntityToSend(SAV9SV sav, PokeTradeDetail<PK9> poke, PK9 offered, byte[] oldEC, PK9 toSend, PartnerDataHolder partnerID, CancellationToken token)
        {
            return poke.Type switch
            {
                PokeTradeType.Random => await HandleRandomLedy(sav, poke, offered, toSend, partnerID, token).ConfigureAwait(false),
                PokeTradeType.Clone => await HandleClone(sav, poke, offered, oldEC, token).ConfigureAwait(false),
                _ => (toSend, PokeTradeResult.Success),
            };
        }

        private async Task<(PK9 toSend, PokeTradeResult check)> HandleClone(SAV9SV sav, PokeTradeDetail<PK9> poke, PK9 offered, byte[] oldEC, CancellationToken token)
        {
            var la = new LegalityAnalysis(offered);
            if (!la.Valid)
            {
                Log($"克隆请求 (来自{poke.Trainer.TrainerName})的不合法宝可梦:{GameInfo.GetStrings(1).Species[offered.Species]}.");
                if (DumpSetting.Dump)
                    DumpPokemon(DumpSetting.DumpFolder, "hacked", offered);

                var report = la.Report();
                Log(report);
                poke.SendNotification(this, "根据PKHeX的合法性检查，这个宝可梦是不合法的。我不能克隆这个。退出交易。");
                poke.SendNotification(this, report);

                return (offered, PokeTradeResult.IllegalTrade);
            }

            var clone = (PK9)offered.Clone();
            if (Hub.Config.Legality.ResetHOMETracker)
                clone.Tracker = 0;

            poke.SendNotification(this, $"**克隆了你的{GameInfo.GetStrings(1).Species[clone.Species]}!**\n现在按B取消你的交换申请，给我一只你不需要的宝可梦。");
            Log($"克隆一个 {GameInfo.GetStrings(1).Species[clone.Species]}. 正在等待用户切换他们的宝可梦...");

            // Separate this out from WaitForPokemonChanged since we compare to old EC from original read.
            var partnerFound = await ReadUntilChanged(TradePartnerOfferedOffset, oldEC, 15_000, 0_200, false, true, token).ConfigureAwait(false);
            if (!partnerFound)
            {
                poke.SendNotification(this, "***嘿，快换，不然我就走了!!!***");
                // They get one more chance.
                partnerFound = await ReadUntilChanged(TradePartnerOfferedOffset, oldEC, 15_000, 0_200, false, true, token).ConfigureAwait(false);
            }

            var pk2 = await ReadUntilPresent(TradePartnerOfferedOffset, 25_000, 1_000, BoxFormatSlotSize, token).ConfigureAwait(false);
            if (!partnerFound || pk2 is null || SearchUtil.HashByDetails(pk2) == SearchUtil.HashByDetails(offered))
            {
                Log("交换对象没有切换他的神奇宝贝。");
                return (offered, PokeTradeResult.TrainerTooSlow);
            }

            await Click(A, 0_800, token).ConfigureAwait(false);
            await SetBoxPokemonAbsolute(BoxStartOffset, clone, token, sav).ConfigureAwait(false);

            return (clone, PokeTradeResult.Success);
        }

        private async Task<(PK9 toSend, PokeTradeResult check)> HandleRandomLedy(SAV9SV sav, PokeTradeDetail<PK9> poke, PK9 offered, PK9 toSend, PartnerDataHolder partner, CancellationToken token)
        {
            // Allow the trade partner to do a Ledy swap.
            var config = Hub.Config.Distribution;
            var trade = Hub.Ledy.GetLedyTrade(offered, partner.TrainerOnlineID, config.LedySpecies);
            if (trade != null)
            {
                if (trade.Type == LedyResponseType.AbuseDetected)
                {
                    var msg = $"发现{partner.TrainerName}因滥用Ledy交易而被检测到。.";
                    if (AbuseSettings.EchoNintendoOnlineIDLedy)
                        msg += $"\nID: {partner.TrainerOnlineID}";
                    if (!string.IsNullOrWhiteSpace(AbuseSettings.LedyAbuseEchoMention))
                        msg = $"{AbuseSettings.LedyAbuseEchoMention} {msg}";
                    EchoUtil.Echo(msg);

                    return (toSend, PokeTradeResult.SuspiciousActivity);
                }

                toSend = trade.Receive;
                poke.TradeData = toSend;

                poke.SendNotification(this, "正在写入请求的宝可梦...");
                await SetBoxPokemonAbsolute(BoxStartOffset, toSend, token, sav).ConfigureAwait(false);
            }
            else if (config.LedyQuitIfNoMatch)
            {
                return (toSend, PokeTradeResult.TrainerRequestBad);
            }

            return (toSend, PokeTradeResult.Success);
        }

        private void WaitAtBarrierIfApplicable(CancellationToken token)
        {
            if (!ShouldWaitAtBarrier)
                return;
            var opt = Hub.Config.Distribution.SynchronizeBots;
            if (opt == BotSyncOption.NoSync)
                return;

            var timeoutAfter = Hub.Config.Distribution.SynchronizeTimeout;
            if (FailedBarrier == 1) // failed last iteration
                timeoutAfter *= 2; // try to re-sync in the event things are too slow.

            var result = Hub.BotSync.Barrier.SignalAndWait(TimeSpan.FromSeconds(timeoutAfter), token);

            if (result)
            {
                FailedBarrier = 0;
                return;
            }

            FailedBarrier++;
            Log($"Barrier同步在 {timeoutAfter}秒后超时. Continuing.");
        }

        /// <summary>
        /// Checks if the barrier needs to get updated to consider this bot.
        /// If it should be considered, it adds it to the barrier if it is not already added.
        /// If it should not be considered, it removes it from the barrier if not already removed.
        /// </summary>
        private void UpdateBarrier(bool shouldWait)
        {
            if (ShouldWaitAtBarrier == shouldWait)
                return; // no change required

            ShouldWaitAtBarrier = shouldWait;
            if (shouldWait)
            {
                Hub.BotSync.Barrier.AddParticipant();
                Log($"加入Barrier. Count: {Hub.BotSync.Barrier.ParticipantCount}");
            }
            else
            {
                Hub.BotSync.Barrier.RemoveParticipant();
                Log($"离开Barrier. Count: {Hub.BotSync.Barrier.ParticipantCount}");
            }
        }

        //ban人，后加
        private static RemoteControlAccess GetReference(string name, ulong id, string comment) => new()
        {
            ID = id,
            Name = name,
            Comment = $"Added automatically on {DateTime.Now:yyyy.MM.dd-hh:mm:ss} ({comment})",
        };

        private string GetDodoURL(byte[] bytes)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", Hub.Config.Dodo.DodoUploadFileUrl);
                MultipartFormDataContent contentFormData = new MultipartFormDataContent();
                contentFormData.Add(new ByteArrayContent(bytes), "file", "image.jpg");
                var requestUri = @"https://botopen.imdodo.com/api/v2/resource/picture/upload";
                var result = client.PostAsync(requestUri, contentFormData).Result.Content.ReadAsStringAsync().Result;
                var a = result.Split("https");
                var b = a[1].Split("jpg");
                var c = "https" + b[0] + "jpg";
                return c;
            }

        }
        private async Task<bool> SetBoxPkmWithSwappedIDDetailsSV(PK9 toSend, TradeMyStatus tradePartner, SAV9SV sav, bool ModId, CancellationToken token)
        {
            if (toSend.Species == (ushort)Species.Ditto)
            {
                Log($"发送百变怪，不进行自id操作");
                return false;
            }
            if (ModId)
            {
                Log($"用户自定义ID，不进行自id操作");
                return false;
            }
            var cln = toSend.Clone();
            cln.OT_Gender = tradePartner.Gender;
            cln.TrainerTID7 = (uint)Math.Abs(tradePartner.DisplayTID);
            cln.TrainerSID7 = (uint)Math.Abs(tradePartner.DisplaySID);
            cln.Language = tradePartner.Language;
            cln.OT_Name = tradePartner.OT;
            if (toSend.IsEgg == false)
            {
                if (toSend.Species == (ushort)Species.Koraidon)
                {
                    cln.Version = (int)GameVersion.SL;
                    Log($"故勒顿，强制修改版本为朱");

                }
                else if (toSend.Species == (ushort)Species.Miraidon)
                {
                    cln.Version = (int)GameVersion.VL;
                    Log($"密勒顿，强制修改版本为紫");
                }
                else
                {
                    cln.Version = tradePartner.Game;
                }
                cln.ClearNickname();
            }
            else
            {
                cln.IsNicknamed = true;
                cln.Nickname = tradePartner.Language switch
                {
                    1 => "タマゴ",
                    3 => "Œuf",
                    4 => "Uovo",
                    5 => "Ei",
                    7 => "Huevo",
                    8 => "알",
                    9 or 10 => "蛋",
                    _ => "Egg",
                };
                Log($"是蛋,修改昵称");
            }
            if (toSend.Met_Location == Locations.TeraCavern9 && toSend.IsShiny)
            {
                cln.PID = (((uint)(cln.TID16 ^ cln.SID16) ^ (cln.PID & 0xFFFF) ^ 1u) << 16) | (cln.PID & 0xFFFF);
            }
            else if (toSend.IsShiny)
                cln.SetShiny();

            cln.RefreshChecksum();

            var tradeSV = new LegalityAnalysis(cln);
            if (tradeSV.Valid)
            {
                Log($"自id后合法，使用自id");
                await SetBoxPokemonAbsolute(BoxStartOffset, cln, token, sav).ConfigureAwait(false);
            }
            else
            {
                Log($"自id后不合法，使用默认id");
            }

            return tradeSV.Valid;
        }
    }
}
