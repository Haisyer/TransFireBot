using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Manganese.Array;
using Mirai.Net.Utils.Scaffolds;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQHelper<T> where T : PKM, new()
    {
        /// <summary>
        /// 精灵交换
        /// </summary>
        public static void StartTrade(string ps, string qq, string nickName, string groupId)
        {
            var _ = CheckAndGetPkm(ps, qq, out var msg, out var pkm,out var modid);
            if (!_)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(msg).Build());
                return;
            }

            StartTradeWithoutCheck(pkm, qq, nickName, groupId,modid);
        }

        /// <summary>
        /// 精灵交换
        /// </summary>
        public static void StartTrade(T pkm, string qq, string nickName, string groupId)
        {
            var _ = CheckPkm(pkm, qq, out var msg);
            if (!_)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(msg).Build());
                return;
            }

            StartTradeWithoutCheck(pkm, qq, nickName, groupId,false);
        }

        /// <summary>
        /// 中文批量交换
        /// </summary>
        public static void StartTradeMultiChinese(string chinesePsRaw, string qq, string nickName, string groupId)
        {
            var chinesePss = chinesePsRaw.Split('+').ToList();
            var MaxPkmsPerTrade = MiraiQQBot<T>.Info.Hub.Config.QQ.qqTradeMaxNumber;
            if (MaxPkmsPerTrade <= 1)
            {
                MiraiQQBot<T>.SendGroupMessage("请联系群主将QQ/qqTradeMaxNumber配置改为大于1");
                return;
            }
            else if (chinesePss.Count > MaxPkmsPerTrade)
            {
                MiraiQQBot<T>.SendGroupMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}");
                return;
            }
           
            int invalidCount = 0;
            string qqNumberPath = qq;
            string userpath = MiraiQQBot<T>.Info.Hub.Config.Folder.TradeFolder + @"\" + qqNumberPath;
            if (Directory.Exists(userpath)) 
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);
            var modId = false;
            var pokemonMessage = "";
            for (var i = 0; i < chinesePss.Count; i++)
            {
                var ps = ShowdownTranslator<T>.Chinese2Showdown(chinesePss[i]);
                var _ = CheckAndGetPkm(ps, qq, out var msg, out var pkm, out var modid);
                modId = modid;
                if (!_)
                {
                    LogUtil.LogInfo($"中文批量:第{i + 1}只宝可梦有问题:{msg}", nameof(MiraiQQHelper<T>));
                    invalidCount++;
                    pokemonMessage += $"\n第{i + 1}只有问题";
                    //防止qq单条消息字数过多发送失败
                    if (pokemonMessage.Length > 1000)
                    {
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                        pokemonMessage = "";
                    }
                }
                else
                {
                    LogUtil.LogInfo($"中文批量:第{i + 1}只:\n{ps}", nameof(MiraiQQHelper<T>));
                    File.WriteAllBytes(userpath + @"\" + $"第{i + 1}只.pk9", pkm.Data);
                   
                }
            }
            if (invalidCount == chinesePss.Count)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n一个都不合法，换个屁").Build());
                //MiraiQQBot<T>.SendGroupMessage("一个都不合法，换个屁");
                return;
            }
            else if (invalidCount != 0)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n交换合法的{chinesePss.Count-invalidCount}只").Build());
            }
            else
            {            
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n开始批量交换{chinesePss.Count}只").Build());
            }

            //var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var code = MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(qq)
                   ? MiraiQQBot<T>.TradeCodeDictionary[qq]
                   : MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(qq), nickName, groupId, RequestSignificance.Favored,
                PokeRoutineType.LinkTrade, out string message, qqNumberPath, true, modId);
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(message).Build());
        }

        /// <summary>
        /// 文件批量交换
        /// </summary>
        public static void StartTradeMulti(List<T> rawPkms, string qq, string nickName, string groupId)
        {
            var MaxPkmsPerTrade = MiraiQQBot<T>.Info.Hub.Config.QQ.qqTradeMaxNumber;
            if (MaxPkmsPerTrade <= 1)
            {
                MiraiQQBot<T>.SendGroupMessage("请联系群主将QQ/qqBinTradeMaxNumber配置改为大于1");
                return;
            }
            else if (rawPkms.Count > MaxPkmsPerTrade)
            {
                MiraiQQBot<T>.SendGroupMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}");
                return;
            }
            
            string qqNumberPath = qq;
            string userpath = MiraiQQBot<T>.Info.Hub.Config.Folder.TradeFolder + @"\" + qqNumberPath;
            if (Directory.Exists(userpath))
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);
            int invalidCount = 0;
            var pokemonMessage = "";
            for (var i = 0; i < rawPkms.Count; i++)
            {
                var _ = CheckPkm(rawPkms[i], qq, out var msg);
                if (!_)
                {
                    LogUtil.LogInfo($"文件批量:第{i + 1}只宝可梦有问题:{msg}", nameof(MiraiQQHelper<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"文件批量:第{i + 1}只:\n{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]}", nameof(MiraiQQHelper<T>));
                    File.WriteAllBytes(userpath + @"\" + $"第{i + 1}只.pk9", rawPkms[i].Data);
                    pokemonMessage += $"\n第{i + 1}只,{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]}";
                    
                    //防止qq单条消息字数过多发送失败
                    if (pokemonMessage.Length > 1000)
                    {
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                        pokemonMessage = "";
                    }
                   
                }
            }
            if (invalidCount == rawPkms.Count)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n一个都不合法，换个屁").Build());
                //MiraiQQBot<T>.SendGroupMessage("一个都不合法，换个屁");
                return;
            }
            else if (invalidCount != 0)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"共读取到{rawPkms.Count}只宝可梦，仅交换合法的{rawPkms.Count - invalidCount}只").Build());
            }
            else
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n开始批量交换{rawPkms.Count}只").Build());
            }
            // var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var code = MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(qq)
                    ? MiraiQQBot<T>.TradeCodeDictionary[qq]
                    : MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(qq), nickName, groupId, RequestSignificance.Favored,
             PokeRoutineType.LinkTrade, out string message, qqNumberPath, true, false);
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(message).Build());
        }

        /// <summary>
        /// ps指令批量交换
        /// </summary>
        public static void StartTradeMultiPs(string pssRaw, string qq, string nickName, string groupId)
        {
            var psArray = pssRaw.Split("\n\n").ToList();
            var MaxPkmsPerTrade = MiraiQQBot<T>.Info.Hub.Config.QQ.qqTradeMaxNumber;
            if (MaxPkmsPerTrade <= 1)
            {
                MiraiQQBot<T>.SendGroupMessage("请联系群主将trade/MaxPkmsPerTrade配置改为大于1");
                return;
            }
            else if (psArray.Count > MaxPkmsPerTrade)
            {
                MiraiQQBot<T>.SendGroupMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}");
                return;
            }
           
            string qqNumberPath = qq;
            string userpath = MiraiQQBot<T>.Info.Hub.Config.Folder.TradeFolder + @"\" + qqNumberPath;
            if (Directory.Exists(userpath))
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);
            int invalidCount = 0;
            var pokemonMessage = "";
            for (var i = 0; i < psArray.Count; i++)
            {
                var ps = psArray[i];
                var _ = CheckAndGetPkm(ps, qq, out var msg, out var pkm,out var modid);
                if (!_)
                {
                    LogUtil.LogInfo($"ps批量:第{i + 1}只宝可梦有问题:{msg}", nameof(MiraiQQHelper<T>));
                    invalidCount++;
                    pokemonMessage += $"\n第{i + 1}只有问题";
                    //防止qq单条消息字数过多发送失败
                    if (pokemonMessage.Length > 1000)
                    {
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                        pokemonMessage = "";
                    }
                }
                else
                {
                    LogUtil.LogInfo($"ps批量:第{i + 1}只:\n{ps}", nameof(MiraiQQHelper<T>));
                    File.WriteAllBytes(userpath + @"\" + $"第{i + 1}只.pk9", pkm.Data);
                }
            }
            if (invalidCount == psArray.Count)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n一个都不合法，换个屁").Build());
                // MiraiQQBot<T>.SendGroupMessage("一个都不合法，换个屁");
                return;
            }
            else if (invalidCount != 0)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(pokemonMessage).Build());
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n有{invalidCount}只宝可梦不合法,仅交换合法的{psArray.Count-invalidCount}只").Build());
            }
            else
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain($"\n开始批量交换{psArray.Count}只宝可梦").Build());
            }

            // var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var code = MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(qq)
                    ? MiraiQQBot<T>.TradeCodeDictionary[qq]
                    : MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(qq), nickName, groupId, RequestSignificance.Favored,
               PokeRoutineType.LinkTrade, out string message, qqNumberPath, true, false);
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(message).Build());
        }

        /// <summary>
        /// 不经过合法检查的交换
        /// </summary>
        public static void StartTradeWithoutCheck(T pkm, string qq, string nickName, string groupId,bool ModID)
        {
            // var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var code = MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(qq)
                     ? MiraiQQBot<T>.TradeCodeDictionary[qq]
                     : MiraiQQBot<T>.Info.GetRandomTradeCode(); 
            var __ = AddToTradeQueue(pkm, code, ulong.Parse(qq), nickName, groupId, RequestSignificance.Favored,
                PokeRoutineType.LinkTrade, out string message, "", false, ModID);
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(message).Build());
        }

        /// <summary>
        /// 开始检测模式
        /// </summary>
        public static void StartDump(string qq, string nickName, string groupId)
        {
            //var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var code = MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(qq)
                   ? MiraiQQBot<T>.TradeCodeDictionary[qq]
                   : MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(qq), nickName, groupId, RequestSignificance.Favored,
                PokeRoutineType.Dump, out string message, "", false, false);
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(message).Build());
        }

        /// <summary>
        /// 检查宝可梦合法性
        /// </summary>
        public static bool CheckPkm(T pkm, string username, out string msg)
        {
            if (!MiraiQQBot<T>.Info.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }
            try
            {
                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, 官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid || MiraiQQBot<T>.Info.Hub.Config.Legality.FileillegalMod)
                    {
                        msg =
                            $"已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = "我没办法创造非法宝可梦";
                msg = $"{reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQBot<T>));
                msg = $"取消派送, 发生了一个错误";
            }

            return false;
        }

        /// <summary>
        /// 检查宝可梦合法性
        /// </summary>
        public static bool CheckAndGetPkm(string setstring, string username, out string msg, out T outPkm,out bool ModID)
        {
            outPkm = new T();
            ModID = false;
            if (setstring.Contains("\n初训家"))
            {
                ModID = true;
                setstring = setstring.Replace("\n初训家", "");
            }
            
            if (!MiraiQQBot<T>.Info.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"取消派送, 宝可梦昵称为空.";
                return false;
            }

            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg =
                    $"取消派送, 请使用正确的Showdown Set代码";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg =
                    $"取消派送, 非法的Showdown Set代码:\n{string.Join("\n", set.InvalidLines)}";
                return false;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);

                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, 官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid || MiraiQQBot<T>.Info.Hub.Config.Legality.CommandillegalMod)
                    {
                        outPkm = pk;

                        msg =
                            $"已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = result == "Timeout"
                    ? "宝可梦创造超时"
                    : "我没办法创造非法宝可梦";
                msg = $"{reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQBot<T>));
                msg = $"取消派送, 发生了一个错误";
            }

            return false;
        }

        /// <summary>
        /// 加入队列
        /// </summary>
        private static bool AddToTradeQueue(T pk, int code, ulong qq, string displayName, string groupID, RequestSignificance sig,
            PokeRoutineType type, out string msg, string path, bool deletFile, bool ModID)
        {
            var userID = qq;
            var name = displayName;
            
            var trainer = new PokeTradeTrainerInfo(name, userID);
            var notifier = new MiraiQQTradeNotifier<T>(pk, trainer, code, name, groupID);
            //var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed : PokeTradeType.Specific;
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed :
                (type == PokeRoutineType.Dump ? PokeTradeType.Dump :
                (type == PokeRoutineType.MutiTrade ? PokeTradeType.MutiTrade :
                PokeTradeType.Specific));
            var detail = new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, sig == RequestSignificance.Favored, path, ModID, deletFile);
            var trade = new TradeEntry<T>(detail, userID, type, name);

            //var added = MiraiQQBot<T>.Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);
            var added = MiraiQQBot<T>.Info.AddToTradeQueue(trade, userID,false);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                //msg = $"@{name}: Sorry, you are already in the queue.";
                msg = $"@{name}: 对不起，您已经在队列中.";
                return false;
            }

            var position = MiraiQQBot<T>.Info.CheckPosition(userID, type);
            //msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";
            msg = $" 你在第{position.Position}位";

            var botct = MiraiQQBot<T>.Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = MiraiQQBot<T>.Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                //msg += $". Estimated: {eta:F1} minutes.";
                msg += $", 需等待约{eta:F1}分钟";
            }

            return true;
        }
    }
}