using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Utils.Scaffolds;
using System.Text.RegularExpressions;
using Mirai.Net.Data.Events.Concretes.Group;
using System.Net.Http;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQBot<T> where T : PKM, new()
    {
        private static PokeTradeHub<T> Hub = default!;

        internal static TradeQueueInfo<T> Info => Hub.Queues.Info;
        internal static readonly List<MiraiQQQueue<T>> QueuePool = new();
        private readonly MiraiBot Client;
        private readonly string GroupId;

        private readonly QQSettings Settings;

        // concurrent?
        internal static ConcurrentDictionary<string, int> TradeCodeDictionary = new();

        public MiraiQQBot(QQSettings settings, PokeTradeHub<T> hub)
        {
            Hub = hub;
            Settings = settings;

            Client = new MiraiBot
            {
                Address = settings.Address,
                QQ = settings.QQ,
                VerifyKey = settings.VerifyKey
            };
            GroupId = settings.GroupId;
            Client.MessageReceived.OfType<GroupMessageReceiver>()
                .Subscribe(async receiver =>
                {
                    try
                    {
                        if (IsBotOrNotTargetGroup(receiver))
                            return;

                        await HandleAliveMessage(receiver);
                        await HandleFileUpload(receiver);
                        await HandleCommand(receiver);
                        await HandlePokemonName(receiver);
                        await HandleCancel(receiver);
                        await HandlePosition(receiver);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogSafe(ex, "MiraiMain");
                        LogUtil.LogError(ex.Message, "MiraiMain");
                    }
                });

            Client.MessageReceived.OfType<TempMessageReceiver>()
                .Subscribe(receiver =>
                {
                    var tradeCode = receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text ?? "";
                    if (Regex.IsMatch(tradeCode, "\\d{8}"))
                    {
                        TradeCodeDictionary[receiver.Sender.Id] = int.Parse(tradeCode);
                    }
                });
            Client.EventReceived.OfType<MemberKickedEvent>()
                .Subscribe(receiver => { Info.ClearTrade(ulong.Parse(receiver.Member.Id)); });
            Client.EventReceived.OfType<MemberLeftEvent>()
                .Subscribe(receiver => { Info.ClearTrade(ulong.Parse(receiver.Member.Id)); });

            Task.Run(async () =>
            {
                try
                {
                    await Client.LaunchAsync();

                    if (!string.IsNullOrWhiteSpace(Settings.MessageStart))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, Settings.MessageStart);
                        await Task.Delay(1_000).ConfigureAwait(false);
                    }

                    if (typeof(T) == typeof(PK8))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为剑盾");
                    }
                    else if (typeof(T) == typeof(PB8))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为晶灿钻石明亮珍珠");
                    }
                    else if (typeof(T) == typeof(PA8))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为阿尔宙斯");
                    }
                    else if (typeof(T) == typeof(PK9))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为朱紫");
                    }

                    await Task.Delay(1_000).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    LogUtil.LogError(ex.Message, nameof(MiraiQQBot<T>));
                }
            });
        }

        private bool IsBotOrNotTargetGroup(GroupMessageReceiver receiver)
        {
            return receiver.Sender.Group.Id != GroupId || receiver.Sender.Id == Settings.QQ;
        }

        private async Task HandleAliveMessage(GroupMessageReceiver receiver)
        {
            if (Settings.AliveMsgOne == receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text)
            {
                await MessageManager.SendGroupMessageAsync(receiver.Sender.Group.Id, Settings.ReplyMsgOne);
                return;
            }
            if (Settings.AliveMsgTwo == receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text)
            {
                await MessageManager.SendGroupMessageAsync(receiver.Sender.Group.Id, Settings.ReplyMsgTwo);
                return;
            }
            if (Settings.AliveMsgThree == receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text)
            {
                await MessageManager.SendGroupMessageAsync(receiver.Sender.Group.Id, Settings.ReplyMsgThree);
                return;
            }
        }

        private async Task HandleCancel(GroupMessageReceiver receiver)
        {
            if (receiver.MessageChain.OfType<AtMessage>().All(x => x.Target != Settings.QQ)) return;
            bool isCancelMsg = (receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text ?? "").Trim()
                .StartsWith("取消");
            if (!isCancelMsg) return;
            var result = Info.ClearTrade(ulong.Parse(receiver.Sender.Id));
            await receiver.SendMessageAsync(
                new AtMessage(receiver.Sender.Id).Append($" {GetClearTradeMessage(result)}"));
        }

        private async Task HandlePosition(GroupMessageReceiver receiver)
        {
            if (receiver.MessageChain.OfType<AtMessage>().All(x => x.Target != Settings.QQ)) return;
            bool isPositionMsg = (receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text ?? "").Trim()
                .StartsWith("位置");
            if (!isPositionMsg) return;
            var result = Info.CheckPosition(ulong.Parse(receiver.Sender.Id));
            await receiver.SendMessageAsync(
                new AtMessage(receiver.Sender.Id).Append($" {GetQueueCheckResultMessage(result)}"));
        }

        public string GetQueueCheckResultMessage(QueueCheckResult<T> result)
        {
            if (!result.InQueue || result.Detail is null)
                return "你不在队列里";
            var msg = $"你在第{result.Position}位";
            var pk = result.Detail.Trade.TradeData;
            if (pk.Species != 0)
                msg += $"，交换宝可梦：{ShowdownTranslator<T>.GameStringsZh.Species[result.Detail.Trade.TradeData.Species]}";
            return msg;
        }

        private static string GetClearTradeMessage(QueueResultRemove result)
        {
            return result switch
            {
                QueueResultRemove.CurrentlyProcessing => "你正在交换中",
                QueueResultRemove.CurrentlyProcessingRemoved => "正在删除",
                QueueResultRemove.Removed => "已删除",
                _ => "你不在队列里",
            };
        }

        private async Task HandlePokemonName(GroupMessageReceiver receiver)
        {
            if (receiver.MessageChain.OfType<AtMessage>().All(x => x.Target != Settings.QQ)) return;
            var text = receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text ?? "";
            if (string.IsNullOrWhiteSpace(text)) return;
            string ps = ShowdownTranslator<T>.Chinese2Showdown(text);
            if (string.IsNullOrWhiteSpace(ps)) return;
            LogUtil.LogInfo($"code\n{ps}", "HandlePokemonName");
            var _ = MiraiQQCommandsHelper<T>.AddToWaitingList(ps, receiver.Sender.Name,
                ulong.Parse(receiver.Sender.Id), out string msg);

            await ProcessAddWaitingListResult(_, msg, receiver.Sender.Id);
        }

        private async Task HandleFileUpload(GroupMessageReceiver receiver)
        {
            var senderQQ = receiver.Sender.Id;
            var groupId = receiver.Sender.Group.Id;

            var fileMessage = receiver.MessageChain.OfType<FileMessage>()?.FirstOrDefault();
            if (fileMessage == null) return;
            LogUtil.LogInfo("In file module", "HandleFileUpload");
            var fileName = fileMessage.Name;
            string operationType;
            if (typeof(T) == typeof(PK8) &&
                fileName.EndsWith(".pk8", StringComparison.OrdinalIgnoreCase)) operationType = "pk8";
            else if (typeof(T) == typeof(PB8) &&
                     fileName.EndsWith(".pb8", StringComparison.OrdinalIgnoreCase))
                operationType = "pb8";
            else if (typeof(T) == typeof(PA8) &&
                     fileName.EndsWith(".pa8", StringComparison.OrdinalIgnoreCase))
                operationType = "pa8";
            else if (typeof(T) == typeof(PK9) &&
                    fileName.EndsWith(".pk9", StringComparison.OrdinalIgnoreCase))
                operationType = "pk9";
            else return;

            PKM pkm;
            try
            {
                var f = await FileManager.GetFileAsync(groupId, fileMessage.FileId, true);

                string url = f.DownloadInfo.Url;
                using var client = new HttpClient();
                var downloadBytes = client.GetByteArrayAsync(url).Result;
                var data = downloadBytes;
                switch (operationType)
                {
                    case "pk8" or "pb8" or "pk9" when data.Length != 344:
                        await MessageManager.SendGroupMessageAsync(groupId, "非法文件");
                        return;
                    case "pa8" when data.Length != 376:
                        await MessageManager.SendGroupMessageAsync(groupId, "非法文件");
                        return;
                }

                switch (operationType)
                {
                    case "pk8":
                        pkm = new PK8(data);
                        break;
                    case "pb8":
                        pkm = new PB8(data);
                        break;
                    case "pa8":
                        pkm = new PA8(data);
                        break;
                    case "pk9":
                        pkm = new PK9(data);
                        break;
                    default: return;
                }

                LogUtil.LogInfo($"operationType:{operationType}", "HandleFileUpload");
                await FileManager.DeleteFileAsync(groupId, fileMessage.FileId);
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, "HandleFileUpload");
                LogUtil.LogError(ex.Message, "HandleFileUpload");
                return;
            }

            var _ = MiraiQQCommandsHelper<T>.AddToWaitingList(pkm, receiver.Sender.Name,
                ulong.Parse(senderQQ), out string msg);
            await ProcessAddWaitingListResult(_, msg, senderQQ);
        }

        private async Task HandleCommand(GroupMessageReceiver receiver)
        {
            string qqMsg;
            try
            {
                qqMsg = receiver.MessageChain.OfType<PlainMessage>().FirstOrDefault()?.Text ?? "";
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(qqMsg) || !qqMsg.Trim().StartsWith("$trade")) return;

            LogUtil.LogInfo($"qqMsg:{qqMsg}", "HandleCommand");
            var split = qqMsg.Split('\n');
            string c = "";
            string args = "";
            string nickName = receiver.Sender.Name;
            string qq = receiver.Sender.Id;
            if (split.Length > 0)
            {
                c = split[0];
                args = qqMsg[(qqMsg.IndexOf('\n') + 1)..];
            }

            switch (c)
            {
                case "$trade":
                    try
                    {
                        await receiver.RecallAsync();
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogSafe(ex, "mirai");
                        LogUtil.LogError($"{ex.Message}", "mirai");
                    }

                    var _ = MiraiQQCommandsHelper<T>.AddToWaitingList(args, nickName, ulong.Parse(qq), out string msg);
                    await ProcessAddWaitingListResult(_, msg, qq);
                    break;
            }
        }

        private async Task ProcessAddWaitingListResult(bool success, string msg, string qq)
        {
            if (success)
            {
                LogUtil.LogInfo(msg, "trade");
                await GetUserFromQueueAndGenerateCodeToTrade(qq);
            }
            else
            {
                LogUtil.LogError(msg, "trade");
                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(qq).Append(" 宝可梦信息异常"));
            }
        }

        private async Task GetUserFromQueueAndGenerateCodeToTrade(string qq)
        {
            var user = QueuePool.FindLast(q => q.QQ == ulong.Parse(qq));

            if (user == null)
                return;
            QueuePool.Remove(user);

            try
            {
                int code = TradeCodeDictionary.ContainsKey(qq)
                    ? TradeCodeDictionary[qq]
                    : Info.GetRandomTradeCode(); //Util.ToInt32(msg);
                var _ = AddToTradeQueue(user.Pokemon, code, user.QQ, user.DisplayName, RequestSignificance.Favored,
                    PokeRoutineType.LinkTrade, out string message);
                if (!_)
                    await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(qq).Append(" 已在队列中"));
                else
                    await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(qq).Append(message));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQBot<T>));
                LogUtil.LogError($"{ex.Message}", nameof(MiraiQQBot<T>));
            }
        }

        private bool AddToTradeQueue(T pk, int code, ulong qq, string displayName, RequestSignificance sig,
            PokeRoutineType type, out string msg)
        {
            var userID = qq;
            var name = displayName;

            var trainer = new PokeTradeTrainerInfo(name, userID);
            var notifier = new MiraiQQTradeNotifier<T>(pk, trainer, code, name, GroupId);
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed : PokeTradeType.Specific;
            var detail = new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, sig == RequestSignificance.Favored);
            var trade = new TradeEntry<T>(detail, userID, type, name);

            var added = Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = $"@{name}: Sorry, you are already in the queue.";
                return false;
            }

            var position = Info.CheckPosition(userID, type);
            //msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";
            msg = $" 你在第{position.Position}位";

            var botct = Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                //msg += $". Estimated: {eta:F1} minutes.";
                msg += $", 需等待约{eta:F1}分钟";
            }

            return true;
        }
    }
}