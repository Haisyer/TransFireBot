using Mirai.Net.Sessions.Http.Managers;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon;
using System;
using System.Linq;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Utils.Scaffolds;
using AHpx.Extensions.StringExtensions;
using Flurl.Util;
using Discord;

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
            var backMethod = Settings.TidAndSidMethod;
            var message_finish = $" 完成\n";
            var message_info = $"收到了：{result.Nickname}\n" +
                           $"原训练家：{result.OT_Name}\n" +
                           $"性别：{gender}\n" +
                           $"Trainer ID：{result.DisplayTID.ToString().PadLeft(6, '0')}\n" +
                           $" Secret ID：{result.DisplaySID.ToString().PadLeft(4, '0')}";

            switch (backMethod)
            {
                //  全不发送
                case 1:
                    SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish));
                    break;
                //  全群发
                case 2:
                    SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish+message_info));
                    break;
                //  全私发
                case 3:
                    SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish));
                    SendTempMessage(message_info);
                    break;
                //  好友私发，非好友不发
                case 4:
                    var qqNumber = info.Trainer.ID.ToString();
                    var res = CheckIsMyFriend(qqNumber);
                    if ( res == true )
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish));
                        SendFriendMessage(message_info);
                    }
                    else
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish));
                    }
                    break;
                //  好友私发，非好友群发
                case 5:
                    var qq = info.Trainer.ID.ToString();
                    var iss = CheckIsMyFriend(qq);
                    if ( iss == true )
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish));
                        SendFriendMessage(message_info);
                    }
                    else
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish+message_info));
                    }
                    break;
                //  其他情况默认不发送
                default:
                    SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_finish));
                    break;

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
            var method = Settings.TradeCodeMethod;
            var message_group = $"准备交换\n连接密码:{info.Code:0000 0000}\n我的名字:{routine.InGameName}";
            var message_temp = $" 准备交换\n连接密码:见私信\n我的名字:{routine.InGameName}";
            var message_friend = $" 准备交换\n连接密码:见私信\n我的名字:{routine.InGameName}";
            var message_code = $" 准备交换\n连接密码是你私信我的\n我的名字: { routine.InGameName}";
            var message_password = $" 连接密码:{info.Code:0000 0000}\n我的名字:{routine.InGameName}";
            var ans = MiraiQQBot<T>.TradeCodeDictionary.ContainsKey(info.Trainer.ID.ToString());
            
            // 下面所有模式均可以私聊机器人交换密码，机器人将使用你发送的交换密码
            switch (method)
            {
                //由机器人群发交换密码
                case 1:
                    if (ans)
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_code));
                    }
                    else
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_group));
                    }
                    break;
                //由机器人向所有人私发交换密码
                case 2:
                    if (ans)
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_code));
                    }
                    else
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_temp));
                        SendTempMessage(message_password);
                    }
                    break;
                //由机器人向其好友私发交换密码，未加好友会取消交易
                case 3:
                    if (ans)
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_code));
                    }
                    else
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_friend));
                        SendTempMessage(message_password);
                    }
                    break;
                //由机器人向其好友私发交换密码，未加好友会发群聊
                case 4:          
                    if (ans)
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_code));
                    }
                    else
                    {                     
                        var qqNumber = info.Trainer.ID.ToString();
                        var flag = CheckIsMyFriend(qqNumber);
                        if( flag == true )
                        {
                            SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_friend));
                            SendFriendMessage(message_password);
                        }
                        else
                        {
                            SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_group));
                        }

                    }
                    break;
                //其他情况默认群发
                default:
                    if (ans)
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_code));
                    }
                    else
                    {
                        SendMessage(new AtMessage($"{info.Trainer.ID}").Append(message_group));
                    }
                    break;
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

        private void SendFriendMessage(string message)
        {
            var qqNumber = Info.ID.ToString();
            var _ = MessageManager.SendFriendMessageAsync(qqNumber, message).Result;       
        }

        private bool CheckIsMyFriend(string qq)
        {
            var qqNumber =qq;
            var allFriends = AccountManager.GetFriendsAsync().Result;
            var eachFriend = allFriends.GetEnumerator();
            //var flag = false;
            while (eachFriend.MoveNext())
            {
                var friendInfo = eachFriend.Current;
                var eachId = friendInfo.Id;               
                if(eachId == qqNumber)
                {
                    return true;
                }
            }
                return false;
        }
    }
}