using System;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQCommandsHelper<T> where T : PKM, new()
    {
        public static bool AddToWaitingList(string setstring, string username, ulong mUserId, out string msg, out T outPkm, out bool ModID)
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
                msg = "对不起，我目前不再接受队列请求!";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"取消派送, @{username}:宝可梦昵称为空.";
                return false;
            }

            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg =
                    $"取消派送, @{username}: 请使用正确的Showdown Set代码.";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg =
                    $"取消派送, @{username}: 非法的Showdown Set代码:\n{string.Join("\n", set.InvalidLines)}";
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
                    msg = $"取消派送, @{username}:官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid || MiraiQQBot<T>.Info.Hub.Config.Legality.CommandillegalMod)
                    {
                        var tq = new MiraiQQQueue<T>(pk, new PokeTradeTrainerInfo(username, mUserId), mUserId);
                        MiraiQQBot<T>.QueuePool.RemoveAll(z => z.QQ == mUserId); // remove old requests if any
                        MiraiQQBot<T>.QueuePool.Add(tq);
                        outPkm = pk;
                        msg =
                            $"@{username} - 已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = result == "Timeout" ? "宝可梦创造超时." : "宝可梦不合法，或者本机器人的数据库未更新.";
                msg = $"取消派送, @{username}: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQCommandsHelper<T>));
                msg = $"取消派送, @{username}:发生了一个预期之外的错误.";
            }

            return false;
        }

        public static bool AddToWaitingList(PKM pkm, string username, ulong mUserId, out string msg)
        {
            if (!MiraiQQBot<T>.Info.GetCanQueue())
            {
                msg = "对不起，我目前不再接受队列请求!";
                return false;
            }

            try
            {
                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, @{username}: 官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid || MiraiQQBot<T>.Info.Hub.Config.Legality.FileillegalMod)
                    {
                        var tq = new MiraiQQQueue<T>(pk, new PokeTradeTrainerInfo(username, mUserId), mUserId);
                        MiraiQQBot<T>.QueuePool.RemoveAll(z => z.QQ == mUserId); // remove old requests if any
                        MiraiQQBot<T>.QueuePool.Add(tq);
                        msg =
                            $"@{username} - 已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = "宝可梦不合法，或者本机器人的数据库未更新.";
                msg = $"取消派送, @{username}: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQCommandsHelper<T>));
                msg = $"取消派送, @{username}: 发生了一个预期之外的错误.";
            }

            return false;
        }
    }
}