using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;

namespace SysBot.Pokemon.Dodo
{
    public class DodoTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private string Username { get; }

        private string ChannelId { get; }

        public DodoTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, string channelId)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            ChannelId = channelId;
            LogUtil.LogText($"Created trade details for {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor<T>> OnFinish { private get; set; }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogText(message);
        }

        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            var line = $"@{info.Trainer.TrainerName}: Trade canceled, {msg}";
            LogUtil.LogText(line);
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, "交换取消", ChannelId);
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = $"@{info.Trainer.TrainerName}: " + (tradedToUser != 0
                ? $"Trade finished. Enjoy your {(Species) tradedToUser}!"
                : "Trade finished!");
            LogUtil.LogText(message);
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, "交换完成", ChannelId);
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg =
                $"@{info.Trainer.TrainerName} (ID: {info.ID}): Initializing trade{receive} with you. Please be ready.";
            msg += $" Your trade code is: {info.Code:0000 0000}";
            LogUtil.LogText(msg);
            var text = $"\n派送:{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}\n密码:见私信\n状态:初始化\n请准备好";
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, text, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(),
                $"派送:{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}\n密码:{info.Code:0000 0000}");
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"I'm waiting for you{trainer}! My IGN is {routine.InGameName}.";
            message += $" Your trade code is: {info.Code:0000 0000}";
            LogUtil.LogText(message);
            var text = $"派送:{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}\n密码:见私信\n状态:搜索中\n";
            DodoBot<T>.SendChannelMessage(text, ChannelId);
            //DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), $"{info.Code:0000 0000}");
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogText(msg);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            var msg = $"Details for {result.FileName}: " + message;
            LogUtil.LogText(msg);
            if (result.Species != 0 && info.Type == PokeTradeType.Dump)
            {
                var text =
                    $"species:{result.Species}\npid:{result.PID}\nec:{result.EncryptionConstant}\nIVs:{string.Join(",", result.IVs)}";
                DodoBot<T>.SendChannelMessage(text, ChannelId);
            }
        }
    }
}