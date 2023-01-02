using System;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQCommandsHelper<T> where T : PKM, new()
    {
        public static bool AddToWaitingList(string setstring, string username, ulong mUserId, out string msg)
        {
            if (!MiraiQQBot<T>.Info.GetCanQueue())
            {
                msg = "Sorry, I am not currently accepting queue requests!";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"Skipping trade, @{username}: Empty nickname provided for the species.";
                return false;
            }

            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg =
                    $"Skipping trade, @{username}: Please read what you are supposed to type as the command argument.";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg =
                    $"Skipping trade, @{username}: Unable to parse Showdown Set:\n{string.Join("\n", set.InvalidLines)}";
                return false;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);

                if (!pkm.CanBeTraded())
                {
                    msg = $"Skipping trade, @{username}: Provided Pokemon content is blocked from trading!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid)
                    {
                        var tq = new MiraiQQQueue<T>(pk, new PokeTradeTrainerInfo(username, mUserId), mUserId);
                        MiraiQQBot<T>.QueuePool.RemoveAll(z => z.QQ == mUserId); // remove old requests if any
                        MiraiQQBot<T>.QueuePool.Add(tq);
                        msg =
                            $"@{username} - added to the waiting list. Your request from the waiting list will be removed if you are too slow!";
                        return true;
                    }
                }

                var reason = result == "Timeout" ? "Set took too long to generate." : "Unable to legalize the Pokemon.";
                msg = $"Skipping trade, @{username}: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQCommandsHelper<T>));
                msg = $"Skipping trade, @{username}: An unexpected problem occurred.";
            }

            return false;
        }

        public static bool AddToWaitingList(PKM pkm, string username, ulong mUserId, out string msg)
        {
            if (!MiraiQQBot<T>.Info.GetCanQueue())
            {
                msg = "Sorry, I am not currently accepting queue requests!";
                return false;
            }

            try
            {
                if (!pkm.CanBeTraded())
                {
                    msg = $"Skipping trade, @{username}: Provided Pokemon content is blocked from trading!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid)
                    {
                        var tq = new MiraiQQQueue<T>(pk, new PokeTradeTrainerInfo(username, mUserId), mUserId);
                        MiraiQQBot<T>.QueuePool.RemoveAll(z => z.QQ == mUserId); // remove old requests if any
                        MiraiQQBot<T>.QueuePool.Add(tq);
                        msg =
                            $"@{username} - added to the waiting list. Your request from the waiting list will be removed if you are too slow!";
                        return true;
                    }
                }

                var reason = "Unable to legalize the Pokemon.";
                msg = $"Skipping trade, @{username}: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQCommandsHelper<T>));
                msg = $"Skipping trade, @{username}: An unexpected problem occurred.";
            }

            return false;
        }
    }
}