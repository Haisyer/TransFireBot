using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace SysBot.Pokemon.Dodo
{
    public class DodoTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private string Username { get; }

        private string ChannelId { get; }

        private string IslandId { get; }

        public DodoTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, string channelId, string islandid)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            ChannelId = channelId;
            IslandId = islandid;
            LogUtil.LogText($"创建交易细节: {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor<T>> OnFinish { private get; set; }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            if (message.Contains("发现连接交换对象:"))
            {
                Regex regex = new Regex("TID: (\\d+)");
                string tid = regex.Match(message).Groups[1].ToString();
                regex = new Regex("SID: (\\d+)");
                string sid = regex.Match(message).Groups[1].ToString();
                var m1 = message.Split(':');
                if (m1.Length > 1)
                {
                    var m2 = m1[1].Split('.');
                    if (m2 != null)
                        DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), $"找到你了{m2[0]}，你本人的表ID:{tid},里ID:{sid}", IslandId);
                }

            }
            else if (message.StartsWith("批量"))
            {
                DodoBot<T>.SendChannelMessage(message, ChannelId);
            }
            else if (CheckWretchName(message))
            {
                DodoBot<T>.SendChannelMessage("**大队长与狗不能进行交换，你家主是不会开机器人吗？** \n **自古忠孝两难全，队长一人成两全**", ChannelId);
            }
            else if (message.StartsWith("该模板"))
            {
                DodoBot<T>.SendChannelMessage(message, ChannelId);
            }
            LogUtil.LogText(message);
        }
        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            string description = GetEnumDescription(msg);
            OnFinish?.Invoke(routine);
            var line = $"交换已取消, 取消原因:{description}";
            LogUtil.LogText(line);
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, line, ChannelId);
            var n = DodoBot<T>.Info.Hub.Config.Queues.AlertNumber;
            for (int i = 1; i <= n; i++)
            {
                var r = DodoBot<T>.Info.CheckIndex(i);
                if (r != 0)
                {
                    DodoBot<T>.SendChannelAtMessage(r, $"请注意,你在第{i + 1}位,{i}个以后该到你了！\n", ChannelId);
                }
            }
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = $"与{info.Trainer.TrainerName}的交易: " + (tradedToUser != 0
                ? $"完成。希望您能与您的{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}玩的愉快!"
                : "完成!");
            var text =
                 $"我收到精灵的种类:{ShowdownTranslator<T>.GameStringsZh.Species[result.Species]}\n" +
                 $"PID:{result.PID:X}\n" +
                 $"加密常数:{result.EncryptionConstant:X}\n" +
                 $"训练家姓名:{result.OT_Name}\n" +
                 $"训练家性别:{(result.OT_Gender == 0 ? "男" : "女")}\n" +
                 $"训练家表ID:{result.TrainerTID7}\n" +
                 $"训练家里ID:{result.TrainerSID7}";
            LogUtil.LogText(message);
            RecordUtil<PokeTradeBotSWSH>.Record($"交换完成\t交换对象:{info.Trainer.TrainerName}\t队列号:{info.ID}\t宝可梦:{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}\t");
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, message, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), text, IslandId);
            var n = DodoBot<T>.Info.Hub.Config.Queues.AlertNumber;
            for (int i = 1; i <= n; i++)
            {
                var r = DodoBot<T>.Info.CheckIndex(i);
                if (r != 0)
                {
                    DodoBot<T>.SendChannelAtMessage(r, $"请注意,你在第{i + 1}位,{i}个以后该到你了！\n", ChannelId);
                }
            }
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg =
                $"正在初始化与{info.Trainer.TrainerName}(ID: {info.ID})的交易{receive}";
            msg += $" 交易密码为: {info.Code:0000 0000}";
            LogUtil.LogText(msg);
            var text = $"队列号:**{info.ID}**\n正在派送:**{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}**\n密码:见私信\n状态:初始化\n请准备好\n";
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, text, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(),
                $"正在派送:{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}\n您的密码:{info.Code:0000 0000}\n{routine.InGameName}正在派送", IslandId);
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"正在等待{name}!,机器人IGN为{routine.InGameName}.";
            message += $" 交换密码为: {info.Code:0000 0000}";
            LogUtil.LogText(message);
            var text = $"我正在等你,第{info.ID}号\n我的游戏ID为{routine.InGameName}\n正在派送:**{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}**\n密码:**见私信**\n状态:搜索中\n";
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, text, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), $"我正在等你,{name}\n密码:{info.Code:0000 0000}\n请速来领取", IslandId);
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
            var msg = $"{result.FileName}的详细信息: " + message;
            LogUtil.LogText(msg);
            string IVstring = "";
            string Abilitystring;
            string GenderString;
            if (message.Contains("检测"))
            {
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
                        $"宝可梦:{ShowdownTranslator<T>.GameStringsZh.Species[result.Species]}({GenderString})\n" +
                        $"个体值:{result.IV_HP},{result.IV_ATK},{result.IV_DEF},{result.IV_SPA},{result.IV_SPD},{result.IV_SPE}" + IVstring + "\n" +
                        $"特性:{Abilitystring}\n" +
                        $"闪光:{(result.IsShiny ? "闪了闪了闪了闪了闪了闪了" : "否")}";
                        
                    var dodoId =ulong.Parse(DodoBot<T>.Info.Hub.Config.Dodo.ClientId);
                    DodoHelper <T> dodoHelper = new DodoHelper<T>(dodoId,Username,ChannelId, IslandId);
                    var eggMsg = dodoHelper.CardInfo(result, out string pokeurl, out string itemurl, out string ballurl, out string teraurl, out string teraoriginurl, out string shinyurl, out string movetypeurl1, out string movetypeurl2, out string movetypeurl3, out string movetypeurl4);
                    string shinyinfo = $"闪光:{(result.IsShiny ? "**!!!闪了!!!闪了!!!**" : "否")}";
                    if (!DodoBot<T>.Info.Hub.Config.Dodo.CardTradeMessage)
                        DodoBot<T>.SendChannelMessage(text, ChannelId);
                    else
                    {
                        DodoBot<T>.SendChannelEggCardMessage(message,eggMsg, ChannelId, pokeurl, ballurl, shinyurl, shinyinfo);
                    }
                }
            }
            else if (message.Contains("https"))
            {
                DodoBot<T>.SendPersonalMessagePicture(message, info.Trainer.ID.ToString(), IslandId);
            }

        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        public bool CheckWretchName(string message)
        {
            string[] banTradeName = { "大队长", "DDZ", "Ddz", "DDz", "dDz", "dDZ", "ddz", "ddZ", "叫我大队长", "我是大队长", "忘世麒麟", "叫我大隊長", "我是大隊長", "大隊長" };
            foreach (var itemName in banTradeName)
            {
                if (message.StartsWith(itemName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
