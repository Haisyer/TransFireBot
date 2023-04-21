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

        private bool DetectionFlag = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public MiraiQQTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, string groupId)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            GroupId = groupId;

            LogUtil.LogText($"创建交易细节: {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }

        /// <summary>
        /// 详情记录
        /// </summary>
        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogText(message);
            //SendMessage($"@{info.Trainer.TrainerName}: {message}");
        }
        /// <summary>
        /// 详情记录
        /// </summary>
        public void SendNotification(PokeRoutineExecutor<T> routine, string message)
        {
            LogUtil.LogText(message);
            //SendMessage($"@{info.Trainer.TrainerName}: {message}");
        }

        /// <summary>
        /// 取消交换
        /// </summary>
        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            var line = $"@{info.Trainer.TrainerName}: 交换取消, {msg}";
            LogUtil.LogText(line);
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(" 取消").Build());
        }
        /// <summary>
        /// 完成交换
        /// </summary>
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
            var backMethod = MiraiQQBot<T>.Info.Hub.Config.QQ.TidAndSidMethod;
            var message_finish = $" 完成\n";
            var message_info = $"收到了：{result.Nickname}\n" +
                           $"原训练家：{result.OT_Name}\n" +
                           $"性别：{gender}\n" +
                           $"Trainer ID：{result.DisplayTID.ToString().PadLeft(6, '0')}\n" +
                           $" Secret ID：{result.DisplaySID.ToString().PadLeft(4, '0')}";

            if (DetectionFlag)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain($"检测完成").Build());
                return;
            }
            switch (backMethod)
            {
                //  全不发送
                case 1:
                    MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish).Build());
                    break;
                //  全群发
                case 2:
                    MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish + message_info).Build());
                    break;
                //  全私发
                case 3:
                    MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish).Build());
                    MiraiQQBot<T>.SendTempMessage(Info.ID.ToString(), message_info);
                    break;
                //  好友私发，非好友不发
                case 4:
                    var qqNumber = info.Trainer.ID.ToString();
                    var res = CheckIsMyFriend(qqNumber);
                    
                    if (res == true)
                    {
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish).Build());
                        //SendFriendMessage(message_info);
                        MiraiQQBot<T>.SendFriendMessage(Info.ID.ToString(), message_info);
                    }
                    else
                    {
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish).Build());
                    }
                    break;
                //  好友私发，非好友群发
                case 5:
                    var qq = info.Trainer.ID.ToString();
                    var iss = CheckIsMyFriend(qq);
                    
                    if (iss == true)
                    {
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish).Build());
                        MiraiQQBot<T>.SendFriendMessage(Info.ID.ToString(), message_info);
                    }
                    else
                    {
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish + message_info).Build());
                    }
                    break;
                //  其他情况默认不发送
                default:
                    MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_finish).Build());
                    break;

            }

        }

        /// <summary>
        /// 初始化并发送交换密码
        /// </summary>
        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg =
                $"@{info.Trainer.TrainerName} (ID: {info.ID}): 正在准备交换给你的:{receive}. 请准备.";
            msg += $" 你的交换密码是: {info.Code:0000 0000}";
            LogUtil.LogText(msg);

            //发送交换密码
            var method = MiraiQQBot<T>.Info.Hub.Config.QQ.TradeCodeMethod;
            var message_group = $"准备交换\n连接密码:{info.Code:0000 0000}\n我的名字:{routine.InGameName}";
            var message_temp = $" 准备交换\n连接密码:见私信\n我的名字:{routine.InGameName}";
            var message_friend = $" 准备交换\n连接密码:见私信\n我的名字:{routine.InGameName}";
            var message_code = $" 准备交换\n连接密码是你私信我的\n我的名字: {routine.InGameName}";
            var message_password = $" 连接密码:{info.Code:0000 0000}\n我的名字:{routine.InGameName}";        

            // 下面所有模式均可以私聊机器人交换密码，机器人将使用你发送的交换密码
            switch (method)
            {
                //由机器人群发交换密码
                case 1:           
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_group).Build());
                        break;
                //由机器人向所有人私发交换密码
                case 2:                   
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_temp).Build());
                        MiraiQQBot<T>.SendTempMessage(Info.ID.ToString(), message_password);
                        break;
                //由机器人向其好友私发交换密码，未加好友会发群聊
                case 3:
                        var qqNumber = info.Trainer.ID.ToString();
                        //var flag = CheckIsMyFriend(qqNumber);
                        bool flag = CheckIsMyFriend(qqNumber);
                        if (flag)
                        {
                            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_friend).Build());
                            MiraiQQBot<T>.SendFriendMessage(Info.ID.ToString(), message_password);
                        }
                        else
                        {
                            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain($"不是好友").Build());
                            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_group).Build());
                        }
                        break;
                //其他情况默认群发
                default:

                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(message_group).Build());
                        break;
            }
        }
        /// <summary>
        /// 是否为bot账号好友
        /// </summary>
        /// <param name="qq"></param>
        /// <returns></returns>
        private bool CheckIsMyFriend(string qq)
        {
            var qqNumber = qq;
            var allFriends = AccountManager.GetFriendsAsync().Result;
            var eachFriend = allFriends.GetEnumerator();
            //var flag = false;
            while (eachFriend.MoveNext())
            {
                var friendInfo = eachFriend.Current;
                var eachId = friendInfo.Id;
                if (eachId == qqNumber)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 寻找交换对象
        /// </summary>
        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"我正在等:{trainer}! 我的名字:{routine.InGameName}.";
            message += $"你的交换密码: {info.Code:0000 0000}";
            LogUtil.LogText(message);
            MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain($" 寻找中").Build());
        }
        /// <summary>
        /// 详情记录
        /// </summary>
        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogText(msg);
            //  MiraiQQBot<T>.SendGroupMessage(msg);
        }
        /// <summary>
        /// 详情记录以及蛋信息的发送
        /// </summary>
        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            var msg = $"它的详情: {result.FileName}: " + message;
            LogUtil.LogText(msg);
           

            string IVstring = "";
            string Abilitystring;
            string GenderString;
            if (message.Contains("检测"))
            {
                DetectionFlag = true;
                if (result.IV_ATK == 0)
                    IVstring = "(0攻)";
                if (result.IV_SPE == 0)
                    IVstring += "(0速)";
                if (result.IVs[0] + result.IVs[1] + result.IVs[2] + result.IVs[3] + result.IVs[4] + result.IVs[5] == 186)
                    IVstring = "(6V)";
                Abilitystring = result.AbilityNumber switch
                {
                    1 => "特性一",
                    2 => "特性二",
                    4 => "梦特",
                    _ => "错误",
                };
                GenderString = result.Gender switch
                {
                    0 => "公",
                    1 => "母",
                    2 => "无性别",
                    _ => "错误",
                };
                if (result.Species != 0 && info.Type == PokeTradeType.Dump)
                {
                    var text = message +
                        $"\n宝可梦:{ShowdownTranslator<T>.GameStringsZh.Species[result.Species]}({GenderString})\n" +
                        $"个体值:{result.IV_HP},{result.IV_ATK},{result.IV_DEF},{result.IV_SPA},{result.IV_SPD},{result.IV_SPE}" + IVstring + "\n" +
                        $"特性:{Abilitystring}\n" +
                        $"闪光:{(result.IsShiny ? "!!!闪光!!!闪光!!!闪光!!!" : "否")}";
                    if (text.Contains("!!!闪光!!!闪光!!!闪光!!!"))
                        MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At($"{info.Trainer.ID}").Plain(text).Build());
                    else
                        MiraiQQBot<T>.SendGroupMessage(text);
                }
            }
        }

    }
}