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

        private readonly QQSettings Settings;

        public MiraiQQTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, string groupId,QQSettings settings)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            GroupId = groupId;
            Settings = settings;

            LogUtil.LogText($"创建交易细节: {Username} - {Code}");
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
            var line = $"@{info.Trainer.TrainerName}: 交换取消, {msg}";
            LogUtil.LogText(line);
            SendMessage(new AtMessage($"{info.Trainer.ID}").Append(" 取消"));
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {        
            OnFinish?.Invoke(routine);
            var gender = result.OT_Gender == 0 ? "男" : "女";
            var tradedToUser = Data.Species;
            //日志
            var message = $"@{info.Trainer.TrainerName}: " + (tradedToUser != 0
                ? $"交换完成,享用你的:{(Species)tradedToUser}!收到:{result.Nickname} 性别:{gender} TID:{result.DisplayTID.ToString().PadLeft(6, '0')} SID:{result.DisplaySID.ToString().PadLeft(4, '0')}"
                : "交换完成!");
            LogUtil.LogText(message);

            //返回交换完成的提示并显示收到的宝可梦信息
            var message1 = $" 完成\n";
            var message2 = $"收到了：{result.Nickname}\n" +
                           $"原训练家：{result.OT_Name}\n" +
                           $"性别：{gender}\n" +
                           $"Trainer ID：{result.DisplayTID.ToString().PadLeft(6, '0')}\n" +
                           $" Secret ID：{result.DisplaySID.ToString().PadLeft(4, '0')}";
            //发送完成提示
            SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message1));

            //发送收到的宝可梦信息
            if (!Settings.TidAndSidMsg)
            {
                SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message2));
            }
            else
            {
                SendTempMessage(message2);
            }
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg =
                $"@{info.Trainer.TrainerName} (ID: {info.ID}): 正在准备交换给你的:{receive}. 请准备.";
            msg += $" 你的交换密码是: {info.Code:0000 0000}";
            LogUtil.LogText(msg);

            //发送交换密码
            var message_temp = $"交换的宝可梦:{Data.Nickname}\n连接密码:{info.Code:0000 0000}\n我的名字:{routine.InGameName}";
            var message_group = $" 准备交换\n连接密码:见私信\n我的名字:{routine.InGameName}";

            if (!Settings.PrivateChatMsg)
            {
                SendMessage(MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(info.Trainer.ID.ToString())
              ? new AtMessage($"{info.Trainer.ID}").Append($" 准备交换\n连接密码是你私信我的\n我的名字:{routine.InGameName}")
              : new AtMessage($"{info.Trainer.ID}").Append(
                  $" 准备交换\n连接密码:{info.Code:0000 0000}\n我的名字:{routine.InGameName}"));
            }
            else
            {
                SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_group));
                SendTempMessage(message_temp);
            }
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"我正在等:{trainer}! 我的名字:{routine.InGameName}.";
            message += $"你的交换密码: {info.Code:0000 0000}";
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
            var msg = $"它的详情: {result.FileName}: " + message;
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

        private void SendTempMessage(string message)
        {
            var qqNumber = Info.ID.ToString();
            var _ = MessageManager.SendTempMessageAsync(qqNumber, GroupId, message).Result;
        }
    }
}