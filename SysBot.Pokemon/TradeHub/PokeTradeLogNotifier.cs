using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;

namespace SysBot.Pokemon
{
    public class PokeTradeLogNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            LogUtil.LogInfo($"开始{info.Trainer.TrainerName}的交换循环, 发送{GameInfo.GetStrings(1).Species[info.TradeData.Species]}", routine.Connection.Label);
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            LogUtil.LogInfo($"搜索与 {info.Trainer.TrainerName}的交换, 发送 {GameInfo.GetStrings(1).Species[info.TradeData.Species]}", routine.Connection.Label);
        }

        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            LogUtil.LogInfo($"因为{msg},取消交换 {info.Trainer.TrainerName}", routine.Connection.Label);
            OnFinish?.Invoke(routine);
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            LogUtil.LogInfo($"完成了{info.Trainer.TrainerName} {GameInfo.GetStrings(1).Species[info.TradeData.Species]}的{GameInfo.GetStrings(1).Species[result.Species]}交换", routine.Connection.Label);
            OnFinish?.Invoke(routine);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogInfo(message, routine.Connection.Label);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogInfo(msg, routine.Connection.Label);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            LogUtil.LogInfo($"通知{info.Trainer.TrainerName}关于他们的{GameInfo.GetStrings(1).Species[result.Species]}", routine.Connection.Label);
            LogUtil.LogInfo(message, routine.Connection.Label);
        }

        public Action<PokeRoutineExecutor<T>>? OnFinish { get; set; }
    }
}