using NLog.Targets;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        /// <summary>
        ///  <para>发送消息的抽象方法</para>
        ///  <para>这个方法不能直接调用,必须在派生类中进行实现</para>
        /// </summary>
        /// <param name="message">发送的消息内容</param>
        public abstract void SendMessage(string message);

        /// <summary>
        /// 发送卡片消息的抽象方法
        /// </summary>
        /// <param name="cardmessage">消息内容</param>
        /// <param name="pokeurl">宝可梦图片地址</param>
        /// <param name="itemurl">道具图片地址</param>
        /// <param name="ballurl">精灵球种图片地址</param>
        /// <param name="teraurl">现钛晶图片地址</param>
        /// <param name="teraoriginalurl">原太晶图片地址</param>
        /// <param name="shinyurl">闪光图片地址</param>
        /// <param name="movetypeurl1">技能一图片地址</param>
        /// <param name="movetypeurl2">技能二图片地址</param>
        /// <param name="movetypeurl3">技能三图片地址</param>
        /// <param name="movetypeurl4">技能四图片地址</param>
        public abstract void SendCardMessage(string cardmessage, string pokeurl, string itemurl, string ballurl, string teraurl, string teraoriginalurl, string shinyurl, string movetypeurl1, string movetypeurl2, string movetypeurl3, string movetypeurl4);
        /// <summary>
        /// 发送卡片消息的抽象方法
        /// </summary>
        /// <param name="cardmessage">消息内容</param>
        /// <param name="pokeurl">宝可梦图片地址</param>
        /// <param name="itemurl">道具图片地址</param>
        /// <param name="ballurl">精灵球种图片地址</param>
        /// <param name="shinyurl">闪光图片地址</param>
        /// <param name="movetypeurl1">技能一图片地址</param>
        /// <param name="movetypeurl2">技能二图片地址</param>
        /// <param name="movetypeurl3">技能三图片地址</param>
        /// <param name="movetypeurl4">技能四图片地址</param>
        public abstract void SendCardBatchMessage(string cardmessage, string pokeurl, string itemurl, string ballurl, string shinyurl);
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
        private LegalitySettings lea = default!;

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
            var _ = CheckAndGetPkm(ps, out var msg, out var pkm, out bool modid);
            if (!_)
            {
                SendMessage(msg);
                return;
            }
            var cardflag = queueInfo.Hub.Config.Dodo.CardTradeMessage;
            if (cardflag)
            {
                var eggurl = GetEggUrl(pkm);
                var cardmsg = CardInfo(pkm, out string pokeurl, out string itemurl, out string ballurl, out string teraurl, out string teraoriginurl, out string shinyurl, out string movetypeurl1, out string movetypeurl2, out string movetypeurl3, out string movetypeurl4);
                if (!string.IsNullOrEmpty(eggurl))
                    pokeurl = eggurl;
                SendCardMessage(cardmsg, pokeurl, itemurl, ballurl, teraurl, teraoriginurl, shinyurl, movetypeurl1, movetypeurl2, movetypeurl3, movetypeurl4);
            }
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, PokeRoutineType.LinkTrade, out string message, modid, "", vip, priority);
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
            StartTradePs(command, vip, priority);
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
            var cardflag = queueInfo.Hub.Config.Dodo.CardTradeMessage;
            if (cardflag)
            {
                var eggurl = GetEggUrl(pkm);
                var cardmsg = CardInfo(pkm, out string pokeurl, out string itemurl, out string ballurl, out string teraurl, out string teraoriginurl, out string shinyurl, out string movetypeurl1, out string movetypeurl2, out string movetypeurl3, out string movetypeurl4);
                if (!string.IsNullOrEmpty(eggurl))
                    pokeurl = eggurl;
                SendCardMessage(cardmsg, pokeurl, itemurl, ballurl, teraurl, teraoriginurl, shinyurl, movetypeurl1, movetypeurl2, movetypeurl3, movetypeurl4);
            }
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, PokeRoutineType.LinkTrade, out string message, false, "", vip, priority);
            SendMessage(message);
        }
        /// <summary>
        /// 开始批量交换宝可梦(ps指令)
        /// </summary>   
        public void StartTradeMultiPs(string pss, string number, bool vip = false, uint priority = uint.MaxValue)
        {
            var psList = pss.Split("\n\n").ToList();

            if (!JudgeMultiNum(psList.Count)) return;
            string numberPath = number;
            string userpath = queueInfo.Hub.Config.Folder.TradeFolder + @"\" + numberPath;
            if (Directory.Exists(userpath))
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);
            GetPKMsFromPsList(psList, userpath, isChinesePS: false, out int invalidCount, out string pokeMessage);
            if (pokeMessage != "")
                SendMessage(pokeMessage);
            if (!JudgeInvalidCount(invalidCount, psList.Count)) return;

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, PokeRoutineType.LinkTrade, out string message, false, numberPath, vip, priority, true);
            SendMessage(message);
        }
        /// <summary>
        ///  开始批量交换宝可梦(中文指令)
        /// </summary>
        public void StartTradeMultiChinesePs(string chinesePssString, string number, bool vip = false, uint priority = uint.MaxValue)
        {
            var chinesePsList = chinesePssString.Split('+').ToList();
            if (!JudgeMultiNum(chinesePsList.Count)) return;

            string numberPath = number;
            string userpath = queueInfo.Hub.Config.Folder.TradeFolder + @"\" + numberPath;
            if (Directory.Exists(userpath))
                Directory.Delete(userpath, true);
            Directory.CreateDirectory(userpath);

            GetPKMsFromPsList(chinesePsList, userpath, true, out int invalidCount, out string pokeMessage);

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
        public void StartTradeMultiPKM(List<T> rawPkms, string number, bool vip = false, uint priority = uint.MaxValue)
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
                    var cardflag = queueInfo.Hub.Config.Dodo.CardTradeMessage;
                    if (cardflag)
                    {
                        var eggurl = GetEggUrl(rawPkms[i]);
                        var cardmsg = CardInfo(rawPkms[i], out string pokeurl, out string itemurl, out string ballurl, out string teraurl, out string teraoriginurl, out string shinyurl, out string movetypeurl1, out string movetypeurl2, out string movetypeurl3, out string movetypeurl4);
                        if (!string.IsNullOrEmpty(eggurl))
                            pokeurl = eggurl;
                        //SendCardMessage(cardmsg, pokeurl, itemurl, ballurl, teraurl, teraoriginurl);
                        if (rawPkms.Count == 1)
                            SendCardMessage(cardmsg, pokeurl, itemurl, ballurl, teraurl, teraoriginurl, shinyurl, movetypeurl1, movetypeurl2, movetypeurl3, movetypeurl4);
                        else
                            SendCardBatchMessage(cardmsg, pokeurl, itemurl, ballurl, shinyurl);
                    }
                    LogUtil.LogInfo($"批量第{i + 1}只:{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]}", nameof(PokemonTradeHelper<T>));
                    File.WriteAllBytes(userpath + @"\" + $"第{i + 1:000}只{version}", rawPkms[i].Data);
                    if (!cardflag)
                    {
                        pokeMessage += $"\n第{i + 1}只,{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]},合法";
                    }

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
            else if (invalidCount == 0)
            {
                SendMessage($"交换合法的{totalCount}只");
            }

            return true;
        }
        /// <summary>
        /// 将列表中的宝可梦写入文件夹中
        /// </summary>
        private void GetPKMsFromPsList(List<string> psList, string userpath, bool isChinesePS, out int invalidCount, out string pokeMessage)
        {
            pokeMessage = "";
            invalidCount = 0;
            var version = CheckVersion();
            if (version == "") return;

            for (var i = 0; i < psList.Count; i++)
            {
                var ps = isChinesePS ? ShowdownTranslator<T>.Chinese2Showdown(psList[i]) : psList[i];
                var _ = CheckAndGetPkm(ps, out var msg, out var pk, out var modid);
                if (!_)
                {
                    invalidCount++;
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(PokemonTradeHelper<T>));
                    pokeMessage += $"\n第{i + 1}只有问题";
                }
                else
                {
                    var cardflag = queueInfo.Hub.Config.Dodo.CardTradeMessage;
                    if (cardflag)
                    {
                        var eggurl = GetEggUrl(pk);
                        var cardmsg = CardInfo(pk, out string pokeurl, out string itemurl, out string ballurl, out string teraurl, out string teraoriginurl, out string shinyurl, out string movetypeurl1, out string movetypeurl2, out string movetypeurl3, out string movetypeurl4);
                        if (!string.IsNullOrEmpty(eggurl))
                            pokeurl = eggurl;
                        //SendCardMessage(cardmsg, pokeurl, itemurl, ballurl, teraurl, teraoriginurl);
                        if (psList.Count == 1)
                            SendCardMessage(cardmsg, pokeurl, itemurl, ballurl, teraurl, teraoriginurl, shinyurl, movetypeurl1, movetypeurl2, movetypeurl3, movetypeurl4);
                        else
                            SendCardBatchMessage(cardmsg, pokeurl, itemurl, ballurl, shinyurl);
                    }
                    File.WriteAllBytes(userpath + @"\" + $"第{i + 1:000}只{version}", pk.Data);
                    LogUtil.LogInfo($"批量第{i + 1}只:\n{ps}", nameof(PokemonTradeHelper<T>));
                    if (!cardflag)
                    { 
                        pokeMessage += $"\n第{i + 1}只,合法"; 
                    }

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
        public bool CheckAndGetPkm(string setstring, out string msg, out T outPkm, out bool ModID)
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
                    LogUtil.LogInfo($"非法判断:{queueInfo.Hub.Config.Legality.PokemonTradeillegalMod}", nameof(PokemonTradeHelper<T>));
                    if (valid || queueInfo.Hub.Config.Legality.PokemonTradeillegalMod)
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


        public void StartTradeWithoutCheck(T pkm, bool modid, string path)
        {
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, PokeRoutineType.LinkTrade, out string message, modid, path);
            SendMessage(message);
        }
        private bool AddToTradeQueue(T pk, int code,
            PokeRoutineType type, out string msg, bool ModId, string path = "", bool vip = false, uint p = uint.MaxValue, bool deletFile = false)
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
            var trade = new TradeEntry<T>(detail, userInfo.ID, type, userInfo.TrainerName);

            var added = queueInfo.AddToTradeQueue(trade, userInfo.ID, vip, p);

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
        public string GetEggUrl(T pk)
        {
            var eggFag = pk.IsEgg;
            string eggurl = "";
            if (eggFag)
                eggurl = "https://img.imdodo.com/openapitest/upload/cdn/E714716E359055F4AD802BD414A97AF2_1696061162810.png";
            return eggurl;
        }
        #region Card Information
        public string CardInfo(T pk, out string pokeurl,out string itemurl, out string ballurl, out string teraurl,out string teraoriginurl, out string shinyurl, out string movetypeurl1, out string movetypeurl2, out string movetypeurl3, out string movetypeurl4)
        {
            string pmsg;
            
            var form = pk.Form;
            int pkForm = pk.Form;

            var species = pk.Species;
            int speciesInt = pk.Species;

            var shiny = pk.IsShiny == true ? "是" : "否";
            int shinyInt = pk.IsShiny == true ? 1 : 0;
            if (shinyInt == 1)
            {
                shinyurl = "https://img.imdodo.com/openapitest/upload/cdn/39240F8E02D5DB05A6686F2E34BACF23_1696445706494.png";
            }
            else
            {
                shinyurl = "https://img.imdodo.com/openapitest/upload/cdn/4A47A0DB6E60853DEDFCFDF08A5CA249_1695595586219.png";
            }

            var pokeball = pk.Ball;
            var prop = pk.HeldItem;           
            var ability = pk.Ability;      
            var natureNumber = pk.Nature;

            var move1 = pk.Move1;
            var move2 = pk.Move2;
            var move3 = pk.Move3;
            var move4 = pk.Move4;

            var type1 = MoveInfo.GetType(move1, pk.Context);
            var type2 = MoveInfo.GetType(move2, pk.Context);
            var type3 = MoveInfo.GetType(move3, pk.Context);
            var type4 = MoveInfo.GetType(move4, pk.Context);

            string tera="";
            string teraoriginal = "";
            teraurl = "https://img.imdodo.com/openapitest/upload/cdn/AEA3F842940BD2E6418AE36231F53BB7_1696061304099.png"; 
            teraoriginurl = "https://img.imdodo.com/openapitest/upload/cdn/AEA3F842940BD2E6418AE36231F53BB7_1696061304099.png";
            
            string hometracker = "";
            string scale ="";
            
            var hp = pk.IV_HP;
            var atk = pk.IV_ATK;
            var def = pk.IV_DEF;
            var spa = pk.IV_SPA;
            var spd = pk.IV_SPD;
            var spe = pk.IV_SPE;
           
            var hp_ev = pk.EV_HP;
            var atk_ev = pk.EV_ATK;
            var def_ev = pk.EV_DEF;
            var spa_ev = pk.EV_SPA;
            var spd_ev = pk.EV_SPD;
            var spe_ev = pk.EV_SPE; 
            
            var gendar = pk.Gender;

            var level = pk.CurrentLevel;
            var version = (GameVersion)pk.Version;
            var versionName = GameInfo.GetVersionName(version);

            Version versionMapper = new Version();
            var chineseVersion = versionMapper.MapToChinese(versionName);


            var power1 = ShowdownTranslator<T>.GameStringsZh.Move[move1].ToString();
            var power2 = ShowdownTranslator<T>.GameStringsZh.Move[move2].ToString();
            var power3 = ShowdownTranslator<T>.GameStringsZh.Move[move3].ToString();
            var power4 = ShowdownTranslator<T>.GameStringsZh.Move[move4].ToString();
            var abilityName = GameInfo.GetStrings("zh").Ability[ability];
            var natureName = GameInfo.GetStrings("zh").Natures[natureNumber];
            try
            {
               ballurl = BallPkImg.ballUrlMapping[pokeball];
                LogUtil.LogInfo($"Ball: {pokeball} is found.", nameof(PokemonTradeHelper<T>));
            }
            catch (KeyNotFoundException)
            {
                LogUtil.LogInfo($"Ball: {pokeball} not found.", nameof(PokemonTradeHelper<T>));
                
               ballurl = "https://img.imdodo.com/openapitest/upload/cdn/AEA3F842940BD2E6418AE36231F53BB7_1696061304099.png";
            }
            try
            {
                itemurl = ItemImg.itemUrlMapping[prop];
                LogUtil.LogInfo($"Item: {prop} is found.", nameof(PokemonTradeHelper<T>));
            }
            catch (KeyNotFoundException)
            {
                LogUtil.LogInfo($"Item: {prop} not found.", nameof(PokemonTradeHelper<T>));
                
                itemurl = "https://img.imdodo.com/openapitest/upload/cdn/4A47A0DB6E60853DEDFCFDF08A5CA249_1695595586219.png";
            }

            MoveTypeImg moveTypeImg = new MoveTypeImg();
            movetypeurl1 = moveTypeImg.MoveTypeToChinese(type1);
            movetypeurl2 = moveTypeImg.MoveTypeToChinese(type2);
            movetypeurl3 = moveTypeImg.MoveTypeToChinese(type3);
            movetypeurl4 = moveTypeImg.MoveTypeToChinese(type4);

            // pokeurl = PKImgURL(speciesint,pkform,shyint);
            pokeurl = "https://img.imdodo.com/openapitest/upload/cdn/AEA3F842940BD2E6418AE36231F53BB7_1696061304099.png";
           
            var key = (Species: speciesInt, Form: pkForm, Shiny: shinyInt);
          //  LogUtil.LogInfo($"KEY:{key}", nameof(PokemonTradeHelper<T>));
            string txtFilePath = "";
            txtFilePath = queueInfo.Hub.Config.Folder.CardImagePath;
            if (txtFilePath != "")
            {
                string[] lines = File.ReadAllLines(txtFilePath);
                
                Regex regex = new Regex(@"\(\s*(\d+),\s*(\d+),\s*(\d+)\)\s*,\s*""(https:\/\/[^""]+)""");

                foreach (var line in lines)
                {
                   
                    Match match = regex.Match(line);
                    if (match.Success)
                    { 
                        int s = int.Parse(match.Groups[1].Value);
                        int f = int.Parse(match.Groups[2].Value);
                        int i = int.Parse(match.Groups[3].Value);
                        string url = match.Groups[4].Value;
                        //var txtKey = (Species: s, Form: f, Shiny: i);
                        //if (txtKey.Equals(key))
                        if (s == speciesInt && f == pkForm && i == shinyInt)
                        {
                            pokeurl = url;
                            LogUtil.LogInfo($"KEY:{key}", nameof(PokemonTradeHelper<T>));
                            LogUtil.LogInfo($"KEY:{url}", nameof(PokemonTradeHelper<T>));                          
                        }
                    }

                }
            }

            if (typeof(T) == typeof(PK8))
            {               
                PK8? pks = FileTradeHelper<T>.GetPokemon(pk.Data) as PK8;
                scale = "无";
                if(pks.Tracker == 0)
                {
                    hometracker = "无追踪码";
                }
                else
                    hometracker = "有追踪码";
            }
            if (typeof(T) == typeof(PA8))
            {
                PA8? pks = FileTradeHelper<T>.GetPokemon(pk.Data) as PA8;
                scale = pks.Scale.ToString();
                if (pks.Tracker == 0)
                {
                    hometracker = "无追踪码";
                }
                else
                    hometracker = "有追踪码";
            }
            if (typeof(T) == typeof(PB8))
            {
                PB8? pks = FileTradeHelper<T>.GetPokemon(pk.Data) as PB8;
                scale = "无";
                if (pks.Tracker == 0)
                {
                    hometracker = "无追踪码";
                }
                else
                    hometracker = "有追踪码";
            }
            if (typeof(T) == typeof(PK9))
            {
                PK9? pks = FileTradeHelper<T>.GetPokemon(pk.Data) as PK9;
                tera=pks.TeraType.ToString();
                teraoriginal = pks.TeraTypeOriginal.ToString();               
                scale = pks.Scale.ToString();
                if (pks.Tracker == 0)
                {
                    hometracker = "无追踪码";
                }
                else
                    hometracker = "有追踪码";

                try
                {
                    teraurl = TeraImage.TeraUrlMapping[tera];
                   // LogUtil.LogInfo($"Tera: {tera} is found.", nameof(PokemonTradeHelper<T>));
                }
                catch (KeyNotFoundException)
                {
                  //  LogUtil.LogInfo($"Tera: {tera} not found.", nameof(PokemonTradeHelper<T>));

                    teraurl = "https://img.imdodo.com/openapitest/upload/cdn/AEA3F842940BD2E6418AE36231F53BB7_1696061304099.png";
                }
                try
                {
                    teraoriginurl = TeraImage.TeraUrlMapping[teraoriginal];
                    // LogUtil.LogInfo($"Tera Original: {teraoriginal} is found.", nameof(PokemonTradeHelper<T>));
                }
                catch (KeyNotFoundException)
                {
                    //  LogUtil.LogInfo($"Tera Original: {teraoriginal} not found.", nameof(PokemonTradeHelper<T>));

                    teraoriginurl = "https://img.imdodo.com/openapitest/upload/cdn/AEA3F842940BD2E6418AE36231F53BB7_1696061304099.png";
                }

            }
            //LogUtil.LogInfo($"itemimage:{itemurl}", nameof(PokemonTradeHelper<T>));
            //LogUtil.LogInfo($"pkimage:{pokeurl}", nameof(PokemonTradeHelper<T>));
            //LogUtil.LogInfo($"ballimage:{ballurl}", nameof(PokemonTradeHelper<T>));
            pmsg = $"**昵称：{GameInfo.GetStrings("zh").Species[species]}**\n" +
                $"性别：{GameInfo.GenderSymbolUnicode[pk.Gender]}\n" +
                $"性格:{natureName}\n" +
                $"特性:{abilityName}\n" +
                $"等级:{level}\n" +
                $"大小:{scale}\n" +
                $"Home追踪:{hometracker}\n" +
                $"个体:\n" +
                $"HP :{hp},Atk:{atk},Def:{def},Spa:{spa},Spd:{spd},Spe:{spe}\n " +
                $"努力:\n" +
                $"HP :{hp_ev},Atk:{atk_ev},Def:{def_ev},Spa:{spa_ev},Spd:{spd_ev},Spe:{spe_ev}\n" +
                $"技能\n" +
                $"{power1}\n" +
                $"{power2}\n" +
                $"{power3}\n" +
                $"{power4}\n" +
                $"来源版本:{chineseVersion}";

          //  LogUtil.LogInfo($"cardmsg:{pmsg}", nameof(PokemonTradeHelper<T>));
            return pmsg;
           
        }
        #endregion
    }
}
