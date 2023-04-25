using NLog.Targets;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SysBot.Pokemon.Helpers
{
    /// <summary>
    /// <para>帮助机器人交换宝可梦的工具类</para>
    /// <para>这是一个抽象类</para>
    /// <para>本类需要实现SendMessage()，同时也要实现一个多参数的构造函数</para>
    /// <para>参数应该包括该类型机器人发送消息的相关信息，以便SendMessage()使用</para>
    /// <para>注意在实现抽象类的构造方法里一定要调用SetPokeTradeTrainerInfo()和SetTradeQueueInfo()</para>
    /// </summary>
    public abstract class PokemonTradeHelper<T> where T : PKM, new()
    {
      
       // public static LegalitySettings Settings = default!;
        internal static LegalitySettings set = default!;
       // public PokemonTradeHelper(LegalitySettings settings) => Settings = settings;

        /// <summary>
        ///  <para>发送消息的抽象方法</para>
        ///  <para>这个方法不能直接调用,必须在派生类中进行实现</para>
        /// </summary>
        /// <param name="message">发送的消息内容</param>
        public abstract void SendMessage(string message);
       
        /// <summary>
        /// <para>获得宝可梦交换信息的抽象方法</para>
        ///  <para>这个方法不能直接调用,必须在派生类中进行实现</para>
        /// </summary>
        /// <param name="pkm">宝可梦实例</param>
        /// <param name="code">密码</param>
        /// <returns></returns>
        public abstract IPokeTradeNotifier<T> GetPokeTradeNotifier(T pkm, int code);
       
        /// <summary>
       /// 用户信息
       /// </summary>
        protected PokeTradeTrainerInfo userInfo = default!;
        
        /// <summary>
        /// 队列信息
        /// </summary>
        private TradeQueueInfo<T> queueInfo = default!;

        /// <summary>
        /// 设定宝可梦交换的用户信息
        /// </summary>
        /// <param name="pokeTradeTrainerInfo">宝可梦交换用户信息</param>
        public void SetPokeTradeTrainerInfo(PokeTradeTrainerInfo pokeTradeTrainerInfo)
        {
            userInfo = pokeTradeTrainerInfo;
        }

        /// <summary>
        /// 设定宝可梦交换的队列信息
        /// </summary>
        /// <param name="queueInfo">宝可梦交换队列信息</param>
        public void SetTradeQueueInfo(TradeQueueInfo<T> queueInfo)
        {
            this.queueInfo = queueInfo;
        }
        public void StartDump()
        {
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, PokeRoutineType.Dump, out string message, false);
           SendMessage(message);
        }
        /// <summary>
        /// 开始交换宝可梦(PokemonShowdown指令)
        /// </summary>
        /// <param name="ps">PokemonShowdown指令</param>
        public void StartTradePs(string ps, bool vip = false, uint priority = uint.MaxValue)
        {
            var _ = CheckAndGetPkm(ps, out var msg, out var pkm,out bool modid);
            if (!_)
            {
                SendMessage(msg);
                return;
            }
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code,PokeRoutineType.LinkTrade, out string message,modid,"",vip, priority);
            SendMessage(message);
        }

        /// <summary>
        /// 开始交换宝可梦(中文指令)
        /// </summary>
        /// <param name="chinesePs">中文指令</param>
        public void StartTradeChinesePs(string chinesePs, bool vip = false, uint priority = uint.MaxValue)
        {
            //LogUtil.LogInfo($"中文转换前ps代码:\n{chinesePs}", nameof(PokemonTradeHelper<T>));
            var command = ShowdownTranslator<T>.Chinese2Showdown(chinesePs);
            LogUtil.LogInfo($"中文转换后ps代码:\n{command}", nameof(PokemonTradeHelper<T>));
            StartTradePs(command,vip,priority);
        }

        /// <summary>
        /// 开始交换宝可梦(pk文件)
        /// </summary>
        public void StartTradePKM(T pkm, bool vip = false, uint priority = uint.MaxValue)
        {
            var _ = CheckPkm(pkm, out var msg);
            if (!_)
            {
                SendMessage(msg);
                return;
            }

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, PokeRoutineType.LinkTrade, out string message, false, "", vip, priority);
            SendMessage(message);
        }
        /// <summary>
        /// 开始批量交换宝可梦(ps指令)
        /// </summary>   
        public void StartTradeMultiPs(string pss,string number, bool vip = false, uint priority = uint.MaxValue)
        {
            var psList = pss.Split("\n\n").ToList();

            if (!JudgeMultiNum(psList.Count)) return;
            string numberPath = number;
            string userpath = queueInfo.Hub.Config.Folder.TradeFolder + @"\" + numberPath;
            if (Directory.Exists(userpath))
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);
            GetPKMsFromPsList(psList, userpath, isChinesePS: false, out int invalidCount,out string pokeMessage);
            if(pokeMessage !="")
                SendMessage(pokeMessage);
            if (!JudgeInvalidCount(invalidCount, psList.Count)) return;

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code,PokeRoutineType.LinkTrade, out string message,false,numberPath, vip, priority, true);
            SendMessage(message);
        }
        /// <summary>
        ///  开始批量交换宝可梦(中文指令)
        /// </summary>
        public void StartTradeMultiChinesePs(string chinesePssString,string number, bool vip = false, uint priority = uint.MaxValue)
        {
            var chinesePsList = chinesePssString.Split('+').ToList();
            if (!JudgeMultiNum(chinesePsList.Count)) return;

            string numberPath = number;
            string userpath = queueInfo.Hub.Config.Folder.TradeFolder + @"\" + numberPath;
            if (Directory.Exists(userpath))
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);

            GetPKMsFromPsList(chinesePsList, userpath, true, out int invalidCount, out string pokeMessage );
           
            if (pokeMessage != "")
                SendMessage(pokeMessage);

            if (!JudgeInvalidCount(invalidCount, chinesePsList.Count)) return;

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, PokeRoutineType.LinkTrade, out string message, false, numberPath, vip, priority, true);
            SendMessage(message);
        }
        /// <summary>
        ///  开始批量交换宝可梦(文件)
        /// </summary>
        public void StartTradeMultiPKM(List<T> rawPkms,string number, bool vip = false, uint priority = uint.MaxValue)
        {
            if (!JudgeMultiNum(rawPkms.Count)) return;
            string numberPath = number;
            string userpath = queueInfo.Hub.Config.Folder.TradeFolder + @"\" + numberPath;
            if (Directory.Exists(userpath))
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);
           
            int invalidCount = 0;
            var version = CheckVersion();
            if (version == "") return;
            LogUtil.LogInfo($"当前版本:{version}", nameof(PokemonTradeHelper<T>));
            string pokeMessage = "";
            for (var i = 0; i < rawPkms.Count; i++)
            {
                var _ = CheckPkm(rawPkms[i], out var msg);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(PokemonTradeHelper<T>));
                    invalidCount++;
                    pokeMessage += $"\n第{i + 1}只宝可梦有问题";
                }
            
                else
                {
                    LogUtil.LogInfo($"批量第{i + 1}只:{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]}", nameof(PokemonTradeHelper<T>));
                    File.WriteAllBytes(userpath + @"\" + $"第{i + 1:000}只{version}", rawPkms[i].Data);
                    pokeMessage += $"\n第{i + 1}只,{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]},合法";
                    
                    if (pokeMessage.Length > 1000)
                    {
                        SendMessage(pokeMessage);
                        pokeMessage = "";
                    }
                }
            }
            if (pokeMessage != "")
                SendMessage(pokeMessage);
            if (!JudgeInvalidCount(invalidCount, rawPkms.Count)) return;

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, PokeRoutineType.LinkTrade, out string message, false, numberPath, vip, priority, true);
            SendMessage(message);
        }
        /// <summary>
        /// 判断批量精灵数量
        /// </summary>
        /// <param name="multiNum">批量数量</param>
        /// <returns>bool</returns>
        private bool JudgeMultiNum(int multiNum)
        {
            var maxPkmsPerTrade = queueInfo.Hub.Config.Queues.MutiMaxNumber;
            if (maxPkmsPerTrade <= 1)
            {
                SendMessage("请联系群主将Queues/MutiMaxNumber配置改为大于1");
                return false;
            }
            else if (multiNum > maxPkmsPerTrade)
            {
                SendMessage($"批量交换宝可梦数量应小于等于{maxPkmsPerTrade}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 判断批量中非法宝可梦数量
        /// </summary>
        /// <param name="invalidCount">非法数量</param>
        /// <param name="totalCount"总数></param>
        /// <returns>bool</returns>
        private bool JudgeInvalidCount(int invalidCount, int totalCount)
        {
            if (invalidCount == totalCount)
            {
                SendMessage("一个都不合法，换个屁");
                return false;
            }
            else if (invalidCount != 0)
            {
                SendMessage($"期望交换的{totalCount}只宝可梦中，有{invalidCount}只不合法，仅交换合法的{totalCount - invalidCount}只");
            }
            else if(invalidCount == 0)
            {
                SendMessage($"交换合法的{totalCount}只");
            }
        
            return true;
        }
        /// <summary>
        /// 将列表中的宝可梦写入文件夹中
        /// </summary>
        private void GetPKMsFromPsList(List<string> psList,string userpath,bool isChinesePS, out int invalidCount ,out string pokeMessage )
        {
            pokeMessage = "";
            invalidCount = 0;
            var version = CheckVersion();
            if (version == "") return;
            
            for (var i = 0; i < psList.Count; i++)
            {
                var ps = isChinesePS ? ShowdownTranslator<T>.Chinese2Showdown(psList[i]) : psList[i];
                var _ = CheckAndGetPkm(ps, out var msg, out var pk,out var modid);
                if (!_)
                {
                    invalidCount ++;
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(PokemonTradeHelper<T>));
                    pokeMessage += $"\n第{i + 1}只有问题";
                }
                else
                {
                    
                    File.WriteAllBytes(userpath + @"\" + $"第{i + 1:000}只{version}", pk.Data);
                    LogUtil.LogInfo($"批量第{i + 1}只:\n{ps}", nameof(PokemonTradeHelper<T>));
                    pokeMessage += $"\n第{i + 1}只,合法";

                    if (pokeMessage.Length > 1000)
                    {
                        SendMessage(pokeMessage);
                        pokeMessage = "";
                    }
                }
                
            }
           
            return;
        }
        /// <summary>
        /// 版本文件后缀判断
        /// </summary>
        /// <returns>版本的文件后缀</returns>
       public string CheckVersion()
        {
            string version = "";
            if (typeof(T) == typeof(PK8))
                version = ".pk8";
            if (typeof(T) == typeof(PB8))
                version = ".pb8";
            if (typeof(T) == typeof(PA8))
                version = ".pa8";
            if (typeof(T) == typeof(PK9))
                version = ".pk9";
            return version;
        }
        /// <summary>
        /// 检测宝可梦合法性
        /// </summary>
        /// <param name="pkm"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool CheckPkm(T pkm, out string msg)
        {
            if (!queueInfo.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }
            return Check(pkm, out msg);
        }

        /// <summary>
        /// 检测宝可梦指令信息并生成宝可梦
        /// </summary>
        /// <param name="setstring">指令内容</param>
        /// <param name="msg">结果信息</param>
        /// <param name="outPkm">生成的宝可梦实例</param>
        /// <returns></returns>
        public bool CheckAndGetPkm(string setstring, out string msg, out T outPkm,out bool ModID)
        {
            outPkm = new T();
            ModID = false;
            if (setstring.Contains("\n初训家"))
            {
                ModID = true;
                setstring = setstring.Replace("\n初训家", "");
            }
            if (!queueInfo.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"取消派送, 宝可梦昵称为空.";
                return false;
            }

            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg =
                    $"取消派送, 请使用正确的Showdown Set代码";
                return false;
            }

            try
            {
               // var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
              //  var pkm = sav.GetLegal(template, out var result);
                //var nickname = pkm.Nickname.ToLower();
                //if (nickname == "egg" && Breeding.CanHatchAsEgg(pkm.Species))
                //    TradeExtensions<T>.EggTrade(pkm, template);


                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                GenerationFix(sav);
                var pkm = sav.GetLegal(template, out var result);
                if (pkm.Nickname.ToLower() == "egg" && Breeding.CanHatchAsEgg(pkm.Species))
                    TradeExtensions<T>.EggTrade(pkm, template);
                if (Check((T)pkm, out msg))
                {
                    outPkm = (T)pkm;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(PokemonTradeHelper<T>));
                msg = $"取消派送, 发生了一个错误";
            }

            return false;
        }
        /// <summary>
        /// 修正世代信息
        /// </summary>
        /// <param name="sav">存档信息</param>
        private static void GenerationFix(ITrainerInfo sav)
        {
            if (typeof(T) == typeof(PK8) || typeof(T) == typeof(PB8) || typeof(T) == typeof(PA8)) sav.GetType().GetProperty("Generation")?.SetValue(sav, 8);
        }
        /// <summary>
        /// 检测宝可梦实例
        /// </summary>
        /// <param name="pkm">宝可梦实例</param>
        /// <param name="msg">结果信息</param>
        /// <returns></returns>
        public bool Check(T pkm, out string msg)
        {
            try
            {
                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, 官方禁止该宝可梦交易!";
                    return false;
                }
                if (pkm is T pk)
                {
                    var la = new LegalityAnalysis(pkm);
                    var valid = la.Valid;
                    if (valid ||set.PokemonTradeillegalMod)
                    {
                        msg = $"已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                    LogUtil.LogInfo($"非法原因:\n{la.Report()}", nameof(PokemonTradeHelper<T>));
                }
                LogUtil.LogInfo($"pkm type:{pkm.GetType()}, T:{typeof(T)}", nameof(PokemonTradeHelper<T>));
                var reason = "我没办法创造非法宝可梦";
                msg = $"{reason}";
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(PokemonTradeHelper<T>));
                msg = $"取消派送, 发生了一个错误";
            }
            return false;
        }


        public void StartTradeWithoutCheck(T pkm,bool modid, string path)
        {
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code,PokeRoutineType.LinkTrade, out string message,modid,path);
            SendMessage(message);
        }
        private bool AddToTradeQueue(T pk, int code,
            PokeRoutineType type, out string msg, bool ModId,string path = "", bool vip = false, uint p = uint.MaxValue, bool deletFile = false)
        {
            if (pk == null)
            {
                msg = $"宝可梦数据为空";
                return false;
            }
           
            var trainer = userInfo;
            var notifier = GetPokeTradeNotifier(pk, code);
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed :
                (type == PokeRoutineType.Dump ? PokeTradeType.Dump :
                (type == PokeRoutineType.MutiTrade ? PokeTradeType.MutiTrade :
                PokeTradeType.Specific));
            var detail = new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, vip, path, ModId, deletFile);
            var trade = new TradeEntry<T>(detail, userInfo.ID, type,userInfo.TrainerName);

            var added = queueInfo.AddToTradeQueue(trade, userInfo.ID, vip,p);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = $"你已经在队列中，请不要重复发送";
                return false;
            }

            var position = queueInfo.CheckPosition(userInfo.ID, type);
            //msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";
            msg = $"你在第{position.Position}位";

            var botct = queueInfo.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = queueInfo.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                //msg += $". Estimated: {eta:F1} minutes.";
                msg += $", 需等待约{eta:F1}分钟";
            }

            return true;
        }
    }
}
