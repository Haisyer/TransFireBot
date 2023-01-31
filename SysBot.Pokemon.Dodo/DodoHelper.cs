using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PKHeX.Core;
using SysBot.Base;
using System.IO;

namespace SysBot.Pokemon.Dodo
{
    public class DodoHelper<T> where T : PKM, new()
    {
        public static void StartTrade(string ps, DodoParameter p,bool vip=false, uint priority = uint.MaxValue)
        {
            var _ = CheckAndGetPkm(ps, p.dodoId, out var msg, out var pkm,out var id);
            if (!_)
            {
                DodoBot<T>.SendChannelMessage(msg, p.channelId);
                return;
            }

            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, ulong.Parse(p.dodoId), p.nickName, p.channelId,
              PokeRoutineType.LinkTrade, out string message,id, p.islandid, "", vip, priority);
            DodoBot<T>.SendChannelMessage(message, p.channelId);
        }

        public static void StartTrade(T pkm, DodoParameter p, bool vip=false,uint priority=uint.MaxValue)
        {
            var _ = CheckPkm(pkm, p.dodoId, out var msg);
            if (!_)
            {
                DodoBot<T>.SendChannelMessage(msg, p.channelId);
                return;
            }
            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, ulong.Parse(p.dodoId), p.nickName, p.channelId,
              PokeRoutineType.LinkTrade, out string message, false,p.islandid, "", vip, priority);
            DodoBot<T>.SendChannelMessage(message, p.channelId);
        }
        public static void StartMutiTrade(DodoParameter p, string path,bool DeletFile)
        {
            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(p.dodoId), p.nickName, p.channelId,
                PokeRoutineType.MutiTrade, out string message,false, p.islandid, path,false,uint.MaxValue,DeletFile);
            DodoBot<T>.SendChannelMessage(message, p.channelId);
        }
        public static void StartDump(DodoParameter p)
        {
            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(p.dodoId), p.nickName, p.channelId,
                PokeRoutineType.Dump, out string message, false, p.islandid);
            DodoBot<T>.SendChannelMessage(message, p.channelId);
        }


        public static bool CheckAndGetPkm(string setstring, string username, out string msg, out T outPkm,out bool ModID)
        {
            outPkm = new T();
            ModID = false;
            if (setstring.Contains("\n初训家"))
            {
                ModID = true;
                setstring=setstring.Replace("\n初训家", "");
            }
            if (DodoBot<T>.Info.Hub.Config.Legality.ReturnShowdownSets == true)
            {
                DodoBot<T>.SendPersonalMessage(username, $"收到命令\n{setstring}");
            }
            LogUtil.LogText(setstring);
            if (!DodoBot<T>.Info.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"取消派送, <@!{username}>: 宝可梦昵称为空.";
                return false;
            }

            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg =
                    $"取消派送, <@!{username}>: 请使用正确的Showdown Set代码";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg =
                    $"取消派送, <@!{username}>: 非法的Showdown Set代码:\n{string.Join("\n", set.InvalidLines)}";
                return false;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);
                var nickname = pkm.Nickname.ToLower();
                if (nickname == "egg" && Breeding.CanHatchAsEgg(pkm.Species))
                    TradeExtensions<T>.EggTrade(pkm, template);
                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, <@!{username}>: 官方禁止该宝可梦交易!";
                    return false;
                }
               
                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid|| DodoBot<T>.Info.Hub.Config.Legality.CommandillegalMod)
                    {
                        outPkm = pk;

                        msg =
                            $"<@!{username}> - 已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = result == "Timeout"
                    ? "宝可梦创造超时"
                    : "宝可梦不合法,或机器人数据库未更新";
                msg = $"<@!{username}>: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(DodoBot<T>));
                msg = $"取消派送, <@!{username}>: 发生了一个错误";
            }

            return false;
        }

        public static bool CheckPkm(PKM pk,string username, out string msg)
        {
            string result="";
            if (!DodoBot<T>.Info.GetCanQueue())
            {
                msg = $"对不起,<@!{username}>,我不再接受队列请求!";
                return false;
            }
            if (pk.Nickname == null)
            {
                msg = $"取消派送, <@!{username}>: 宝可梦昵称为空.";
                return false;
            }
            if (pk.Species < 1)
            {
                msg =
                    $"取消派送, <@!{username}>: 请使用正确的Showdown Set代码";
                return false;
            }
            try
            {
            if (!pk.CanBeTraded())
            {
                msg = $"取消派送, <@!{username}>: 官方禁止该宝可梦交易!";
                return false;
            }
            var valid = new LegalityAnalysis(pk).Valid;
            if (valid || DodoBot<T>.Info.Hub.Config.Legality.FileillegalMod)
            {    
                msg =
                    $"<@!{username}> - 已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                return true;
            }
            var reason = result == "Timeout"
                ? "宝可梦创造超时"
                : "宝可梦不合法,或机器人数据库未更新";
            msg = $"<@!{username}>: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(DodoBot<T>));
                msg = $"取消派送, <@!{username}>: 发生了一个错误";
            }

            return false;
        }
        private static bool AddToTradeQueue(T pk, int code, ulong userId, string name, string channelId, 
            PokeRoutineType type, out string msg, bool ModId,string islandid, string path="",bool vip=false,uint p= uint.MaxValue,bool deletFile=false)
        {
            var trainer = new PokeTradeTrainerInfo(name, userId);
            var notifier = new DodoTradeNotifier<T>(pk, trainer, code, name, channelId,islandid);
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed :
                (type == PokeRoutineType.Dump ? PokeTradeType.Dump :
                (type == PokeRoutineType.MutiTrade ? PokeTradeType.MutiTrade :
                PokeTradeType.Specific));
            var detail =
                 new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, vip, path,ModId,deletFile);
            var trade = new TradeEntry<T>(detail, userId, type, name);

            var added = DodoBot<T>.Info.AddToTradeQueue(trade, userId, vip,p);


            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = $"<@!{userId}> 我知道你很急,但你先别急,你已经在队列中，请不要重复发送";
                return false;
            }

            var position = DodoBot<T>.Info.CheckPosition(userId, type);
            
            //msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";
            msg = $" <@!{userId}>:你已经在***{type}***队列,识别码:***{detail.ID}***,你在第***{position.Position}***位";

            var botct = DodoBot<T>.Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = DodoBot<T>.Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                msg += $", 需等待约***{eta:F1}***分钟";
            }

            return true;
        }
      
    }
}