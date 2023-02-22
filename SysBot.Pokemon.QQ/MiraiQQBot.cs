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
using System.IO;
using Discord;

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

            var showdownCodeArray = Regex.Split(text, "[+]+");
            var maxNumber = Settings.qqBatchTradeMaxNumber;
            bool selfswitch = false;

            if (showdownCodeArray.Length > 1)
            {
                if (Settings.BatchTradeSwitch == false)
                {
                    await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n批量交换你把握不住的,洗洗睡吧\n"));
                    return;
                }
                if (maxNumber <= 1)
                {
                    await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n请将菜单qq设置中的qqBatchTradeMaxNumber参数改为大于1\n"));
                    return;
                }
                if (showdownCodeArray.Length > maxNumber)
                {
                    await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"批量交换宝可梦数量应小于等于{maxNumber}"));
                    return;
                }
                    
                    int legalNumber = 0;
                    int illegalNumber = 0;
                    string qqNumberPath = receiver.Sender.Id;
                    string userpath = MiraiQQBot<T>.Info.Hub.Config.Folder.TradeFolder + @"\" + qqNumberPath;
                    string tradepath = MiraiQQBot<T>.Info.Hub.Config.Folder.TradeSaveFile + @"\" + qqNumberPath;
                    string batchtradeMessage = "";
                if (Directory.Exists(tradepath)) Directory.Delete(tradepath, true);
                     Directory.CreateDirectory(tradepath);

                if (Directory.Exists(userpath)) Directory.Delete(userpath, true);
                    Directory.CreateDirectory(userpath);

                    for (legalNumber = 0; legalNumber < showdownCodeArray.Length; legalNumber++)
                    {
                        var PokeInfo = ShowdownTranslator<T>.Chinese2Showdown(showdownCodeArray[legalNumber]);
                        LogUtil.LogInfo($"收到命令\n第{legalNumber + 1}只\n{PokeInfo}\n", "qqBot");
                        var flag = MiraiQQCommandsHelper<T>.AddToWaitingList(PokeInfo, receiver.Sender.Name, ulong.Parse(receiver.Sender.Id), out string mess, out var pkmsg, out var ModID);
                        selfswitch = ModID;

                    if (flag)
                        {
                            LogUtil.LogInfo($" \n第{legalNumber + 1}只合法", "qqBot");
                            if (Settings.BatchFile != false)
                            {                           
                             File.WriteAllBytes(tradepath + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + $"第{legalNumber + 1}只.pk9", pkmsg.Data);                            
                            }
                            File.WriteAllBytes(userpath + @"\" + $"第{legalNumber + 1}只.pk9", pkmsg.Data);
                        }
                        else
                        {
                            ++illegalNumber;
                            batchtradeMessage += $"\n第{legalNumber + 1}只不合法";
                           // await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n第{legalNumber + 1}只不合法"));                          
                            LogUtil.LogInfo($" \n第{legalNumber + 1}只不合法", "qqBot");  
                        
                            //防止qq单条消息过大发不出去
                            if (batchtradeMessage.Length > 1000)
                            {
                                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(receiver.Sender.Id).Append(batchtradeMessage));
                                batchtradeMessage = "";
                            }
                        }

                    }
                    if (legalNumber == illegalNumber)
                    {
                        await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n全不合法，你换个头！"));
                        return;
                    }

                var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
                var __ = AddToTradeQueue(new T(), code, ulong.Parse(receiver.Sender.Id), receiver.Sender.Name, RequestSignificance.Favored,
                  PokeRoutineType.LinkTrade, out string message, qqNumberPath, true, selfswitch);

                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(receiver.Sender.Id).Append(batchtradeMessage));
                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(receiver.Sender.Id).Append($"开始批量交换{legalNumber - illegalNumber}只"));
                text = null;
            }

            string ps = ShowdownTranslator<T>.Chinese2Showdown(text);
            if (string.IsNullOrWhiteSpace(ps)) return;
            LogUtil.LogInfo($"code\n{ps}", "HandlePokemonName");
            var _ = MiraiQQCommandsHelper<T>.AddToWaitingList(ps, receiver.Sender.Name,
                ulong.Parse(receiver.Sender.Id), out string msg, out var pkm, out var ModID2);
            
            await ProcessAddWaitingListResult(_, msg, receiver.Sender.Id,ModID2);
        }

        private async Task HandleFileUpload(GroupMessageReceiver receiver)
        {
            var senderQQ = receiver.Sender.Id;
            var groupId = receiver.Sender.Group.Id;

            var fileMessage = receiver.MessageChain.OfType<FileMessage>()?.FirstOrDefault();
            if (fileMessage == null) return;
            LogUtil.LogInfo("QQBot 文件模式", "HandleFileUpload");
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

            else if (typeof(T) == typeof(PK9) &&
                   fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                operationType = "binpk9";
            else return;
            
            if (operationType == "binpk9")
            {
                    if (Settings.BinTradeSwitch == false)
                    {
                        await FileManager.DeleteFileAsync(groupId, fileMessage.FileId);
                        await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n批量交换你把握不住的,洗洗睡吧\n"));
                        return;
                    }

                    LogUtil.LogInfo("QQBot bin文件模式", "HandleFileUpload");
                    var bin = await FileManager.GetFileAsync(groupId, fileMessage.FileId, true);
                    string binUrl = bin.DownloadInfo.Url;
                    using var binClient = new HttpClient();

                    var downloadBinBytes = binClient.GetByteArrayAsync(binUrl).Result;
                    if (downloadBinBytes.Length != 330240)
                    {
                        await FileManager.DeleteFileAsync(groupId, fileMessage.FileId);
                        await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n你的bin文件有问题"));
                        return;
                    }                     
                    var copyBinBytes = new byte[400];
                    string binMessage = "\n可交换收到的:";

                    int count = 0;  //交换的总宝可梦数量
                    int legalcount = 0; //能够合法交换的宝可梦数量
                    string qqNumberPath = receiver.Sender.Id;
                    string userpath = MiraiQQBot<T>.Info.Hub.Config.Folder.TradeFolder + @"\" + qqNumberPath;
                    string tradepath = MiraiQQBot<T>.Info.Hub.Config.Folder.TradeSaveFile + @"\" + qqNumberPath;

                    if (Directory.Exists(userpath)) Directory.Delete(userpath, true);
                    Directory.CreateDirectory(userpath);
                    if (Directory.Exists(tradepath)) Directory.Delete(tradepath, true);
                    Directory.CreateDirectory(tradepath);

                    int maxnumber = Settings.qqBinTradeMaxNumber;   //单次交换宝可梦的最大数量
                    int tradelength = maxnumber * 344;  //单次交换宝可梦信息的最大字节
                    if (tradelength < 344) tradelength = 344;
                    if (tradelength > 330240) tradelength = 330240;
                   
                    // for (int i = 0; i < downloadBinBytes.Length; i += 344)
                    for (int i = 0; i < tradelength; i += 344)
                    {
                        count++;
                        Buffer.BlockCopy(downloadBinBytes, i, copyBinBytes, 0, 344);
                        var eachdata = copyBinBytes;
                   
                        PKM pokemon;
                        pokemon = new PK9(eachdata);
                        var ans = MiraiQQCommandsHelper<T>.AddToWaitingList(pokemon, receiver.Sender.Name,
               ulong.Parse(senderQQ), out string information);
                        if (ans)
                        {
                            legalcount++;
                             LogUtil.LogInfo($"bin模式:{pokemon.Nickname}", "HandleBinFile");

                            if (Settings.BatchBinFile != false)
                            {
                                File.WriteAllBytes(tradepath + @"\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + $"第{count}只.pk9", pokemon.Data);
                                //await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n第{i/344+1}只,{pokemon.Nickname}"));                      
                            }
                                File.WriteAllBytes(userpath + @"\" + $"第{count}只.pk9", pokemon.Data);
                                // await MessageManager.SendGroupMessageAsync(GroupId, $"\n即将交换第{count}只,{pokemon.Nickname}");
                            binMessage += $"\n第{count}只,{pokemon.Nickname}";
                            
                            //防止qq单条消息过大发不出去
                            if(binMessage.Length > 1000)
                            {                               
                                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(receiver.Sender.Id).Append(binMessage));
                                binMessage = "";
                            }
                        }
                    }
                if (legalcount == 0)
                {
                    await FileManager.DeleteFileAsync(groupId, fileMessage.FileId);
                    await receiver.SendMessageAsync(new AtMessage(receiver.Sender.Id).Append($"\n全不合法，你换个头！"));
                    return;
                }

                await FileManager.DeleteFileAsync(groupId, fileMessage.FileId);
                var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
                var __ = AddToTradeQueue(new T(), code, ulong.Parse(receiver.Sender.Id), receiver.Sender.Name, RequestSignificance.Favored,
                  PokeRoutineType.LinkTrade, out string message, qqNumberPath, true, false);
               
                //await MessageManager.SendGroupMessageAsync(GroupId,binMessage);
                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(receiver.Sender.Id).Append(binMessage));
                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(receiver.Sender.Id).Append($"开始批量交换合法的{legalcount}只"));
                return;
            }

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
            await ProcessAddWaitingListResult(_, msg, senderQQ,false);
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

                    var _ = MiraiQQCommandsHelper<T>.AddToWaitingList(args, nickName, ulong.Parse(qq), out string msg,out var pkm, out var ModID3);
                    await ProcessAddWaitingListResult(_, msg, qq, ModID3);
                    break;
            }
        }

        private async Task ProcessAddWaitingListResult(bool success, string msg, string qq, bool ModID)
        {
            var modid = ModID;
            if (success)
            {
                LogUtil.LogInfo(msg, "trade");
                await GetUserFromQueueAndGenerateCodeToTrade(qq,ModID);
            }
            else
            {
                LogUtil.LogError(msg, "trade");
                await MessageManager.SendGroupMessageAsync(GroupId, new AtMessage(qq).Append(" 宝可梦信息异常"));
            }
        }

        private async Task GetUserFromQueueAndGenerateCodeToTrade(string qq,bool ModID)
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
                    PokeRoutineType.LinkTrade, out string message,"",false, ModID);
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
            PokeRoutineType type, out string msg, string path, bool deletFile,bool ModID)
        {
            var userID = qq;
            var name = displayName;

            var trainer = new PokeTradeTrainerInfo(name, userID);
            var notifier = new MiraiQQTradeNotifier<T>(pk, trainer, code, name, GroupId,Settings);
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed : PokeTradeType.Specific;
            var detail = new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, sig == RequestSignificance.Favored,path,ModID,deletFile);
            var trade = new TradeEntry<T>(detail, userID, type, name);

            var added = Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                //msg = $"@{name}: Sorry, you are already in the queue.";
                msg = $"@{name}: 对不起，您已经在队列中.";
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
