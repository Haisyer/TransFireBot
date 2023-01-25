using Mirai.Net.Sessions.Http.Managers;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon;
using System;
using System.Linq;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Utils.Scaffolds;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private string Username { get; }

        private string GroupId { get; }

        public MiraiQQTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, string groupId)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            GroupId = groupId;
            LogUtil.LogText($"Created trade details for {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogText(message);
            //SendMessage($"@{info.Trainer.TrainerName}: {message}");
        }
        public void SendNotification(PokeRoutineExecutor<T> routine,  string message)
        {
            LogUtil.LogText(message);
            //SendMessage($"@{info.Trainer.TrainerName}: {message}");
        }
        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            var line = $"@{info.Trainer.TrainerName}: Trade canceled, {msg}";
            LogUtil.LogText(line);
            SendMessage(new AtMessage($"{info.Trainer.ID}").Append(" 取消"));
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = $"@{info.Trainer.TrainerName}: " + (tradedToUser != 0
                ? $"Trade finished. Enjoy your {(Species) tradedToUser}!"
                : "Trade finished!");
            LogUtil.LogText(message);
            SendMessage(new AtMessage($"{info.Trainer.ID}").Append(" 完成"));
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg =
                $"@{info.Trainer.TrainerName} (ID: {info.ID}): Initializing trade{receive} with you. Please be ready.";
            msg += $" Your trade code is: {info.Code:0000 0000}";
            LogUtil.LogText(msg);
            SendMessage(MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(info.Trainer.ID.ToString())
                ? new AtMessage($"{info.Trainer.ID}").Append($" 准备交换\n连接密码是你私信我的\n我的名字:{routine.InGameName}")
                : new AtMessage($"{info.Trainer.ID}").Append(
                    $" 准备交换\n连接密码:{info.Code:0000 0000}\n我的名字:{routine.InGameName}"));
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"I'm waiting for you{trainer}! My IGN is {routine.InGameName}.";
            message += $" Your trade code is: {info.Code:0000 0000}";
            LogUtil.LogText(message);
            SendMessage(new AtMessage($"{info.Trainer.ID}").Append($" 寻找中"));
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogText(msg);
            SendMessage(msg);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            var msg = $"Details for {result.FileName}: " + message;
            LogUtil.LogText(msg);
            SendMessage(msg);
        }

        private void SendMessage(string message)
        {
            var _ = MessageManager.SendGroupMessageAsync(GroupId, message).Result;
            LogUtil.LogInfo($"msgId:{_} {message}", "debug");
        }

        private void SendMessage(MessageBase[] message)
        {
            var _ = MessageManager.SendGroupMessageAsync(GroupId, message).Result;
        }
    }
}