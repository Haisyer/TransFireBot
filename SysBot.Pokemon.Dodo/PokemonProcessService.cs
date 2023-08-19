using System;
using DoDo.Open.Sdk.Models.Bots;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Members;
using DoDo.Open.Sdk.Services;
using PKHeX.Core;
using SysBot.Base;
using System.Net.Http;

using SysBot.Pokemon.Helpers;
using DoDo.Open.Sdk.Models.ChannelMessages;
using System.Text.RegularExpressions;

namespace SysBot.Pokemon.Dodo
{
    public class PokemonProcessService<TP> : EventProcessService where TP : PKM, new()
    {
        private readonly OpenApiService _openApiService;
        private static readonly string LogIdentity = "DodoBot";
        private static readonly string Welcome = "宝可梦机器人为您服务\n中文指令请看在线文件:https://docs.qq.com/doc/DVWdQdXJPWllabm5t?&u=1c5a2618155548239a9563e9f22a57c0\n或者使用PS代码\n或者上传pk文件\n取消排队请输入:取消\n当前位置请输入:位置";
        private DodoSettings _dodoSettings;
        private readonly string _channelId;
        private string _botDodoSourceId =default!;
        private uint Count = 0;
      
        public PokemonProcessService(OpenApiService openApiService,DodoSettings settings)
        {
            _openApiService = openApiService;
            _channelId = settings.ChannelId;
            _dodoSettings = settings;
        }


        public override void Connected(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void Disconnected(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void Reconnected(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void Exception(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void PersonalMessageEvent<T>(
           EventSubjectOutput<EventSubjectDataBusiness<EventBodyPersonalMessage<T>>> input)
        {
            var eventBody = input.Data.EventBody;

            if (eventBody.MessageBody is MessageBodyText messageBodyText)
            {
                DodoBot<TP>.SendPersonalMessage(eventBody.DodoSourceId, $"你好", eventBody.IslandSourceId);
            }
        }


        public override void ChannelMessageEvent<T>(
            EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input)
        {
            var eventBody = input.Data.EventBody;
            if (!string.IsNullOrWhiteSpace(_channelId) && eventBody.ChannelId != _channelId) return;

            //};
            if (Count > 100)
            {
                Count = 0;
            }
            var Roleinput = new GetMemberRoleListInput()
            {
                DodoSourceId = eventBody.DodoSourceId,
                IslandSourceId = eventBody.IslandSourceId,
            };
            var Roleoutput = DodoBot<TP>.OpenApiService.GetMemberRoleList(Roleinput);
            bool VipRole = Roleoutput.Exists(x => x.RoleId == DodoBot<TP>.Info.Hub.Config.Dodo.VipRole);
            bool BatchRole = Roleoutput.Exists(x => x.RoleId == DodoBot<TP>.Info.Hub.Config.Dodo.BatchRole);

            //文件交换
            if (eventBody.MessageBody is MessageBodyFile messageBodyFile)
            {
                if (messageBodyFile.Name.Length > 8 && Regex.IsMatch(messageBodyFile.Name[..8], "^(?<year>\\d{4})(?<month>\\d{2})(?<day>\\d{2})$"))
                {
                    DodoBot<TP>.SendChannelMessage("**大队长与狗不得使用**", eventBody.ChannelId);
                    MemberMuteAdd(eventBody.IslandSourceId, eventBody.DodoSourceId, 604800, "使用了大队长的文件，请你回他的频道使用享受12小时CD捏QwQ");
                    return;
                }
                if (!FileTradeHelper<TP>.IsValidFileSize(messageBodyFile.Size ?? 0) || !FileTradeHelper<TP>.IsValidFileName(messageBodyFile.Name))
                {
                    ProcessWithdraw(eventBody.MessageId);
                    DodoBot<TP>.SendChannelMessage("非法文件", eventBody.ChannelId);
                    MemberMuteAdd(eventBody.IslandSourceId, eventBody.DodoSourceId, 600, "使用非法文件,关你几分钟小黑屋QAQ");
                    return;
                }
                using var client = new HttpClient();
                var downloadBytes = client.GetByteArrayAsync(messageBodyFile.Url).Result;
                var pkms = FileTradeHelper<TP>.DataToList(downloadBytes);
                ProcessWithdraw(eventBody.MessageId);
                if (pkms.Count == 1)
                {
                    if (VipRole)
                    {
                        DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                        new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradePKM(pkms[0], true, Count);
                        Count++;
                    }
                    else
                        new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradePKM(pkms[0]);
                }
                else if (pkms.Count > 1 && pkms.Count <= FileTradeHelper<TP>.maxPokemonCountInBin)
                {
                    if (!BatchRole && !VipRole)
                        DodoBot<TP>.SendChannelMessage("你没有批量权限", eventBody.ChannelId);   
                    else
                    {
                        if (VipRole)
                        {
                            DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                            new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiPKM(pkms, eventBody.DodoSourceId, true, Count);
                            Count++;
                        }
                        else
                            new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiPKM(pkms, eventBody.DodoSourceId);
                    }
                }
                else
                    DodoBot<TP>.SendChannelMessage("文件内容不正确", eventBody.ChannelId);
                return;
            }
            //指令交换
            if (eventBody.MessageBody is not MessageBodyText messageBodyText) return;

            var content = messageBodyText.Content;

            LogUtil.LogInfo($"{eventBody.Personal.NickName}({eventBody.DodoSourceId}):{content}", LogIdentity);
            if (_botDodoSourceId == null)
            {
                _botDodoSourceId = _openApiService.GetBotInfo(new GetBotInfoInput()).DodoSourceId;
            }
            if (!content.Contains($"<@!{_botDodoSourceId}>")) return;

            content = content.Substring(content.IndexOf('>') + 1);
           
            if (ShowdownTranslator<TP>.IsPS(content) && content.Contains("\n\n"))
            // if (typeof(TP) == typeof(PK9) && content.Contains("\n\n") && ShowdownTranslator<TP>.IsPS(content))
            {
               // if (typeof(TP) != typeof(PK9) && typeof(TP) != typeof(PA8)) return;//全版本后即可删除
                ProcessWithdraw(eventBody.MessageId);
                if(!BatchRole && !VipRole) 
                {
                    DodoBot<TP>.SendChannelMessage("你没有批量权限", eventBody.ChannelId);
                }
                else
                {
                    if (VipRole)
                    {
                        DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                        new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiPs(content.Trim(), eventBody.DodoSourceId,true,Count);
                        Count++;
                    }
                    else
                        new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiPs(content.Trim(), eventBody.DodoSourceId);

                }
                return;
            }
            else if (ShowdownTranslator<TP>.IsPS(content))
            {
                ProcessWithdraw(eventBody.MessageId);
                if (VipRole)
                {
                    DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                    new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradePs(content.Trim(),true,Count);
                    Count++;
                }
                else
                    new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradePs(content.Trim());

                return;
            }

            else if (content.Trim().StartsWith("检测"))
            {
                ProcessWithdraw(eventBody.MessageId);
                new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartDump();
                return;
            }

            else if ( content.Trim().Contains('+'))          
            //else if (typeof(TP) == typeof(PK9) && content.Trim().Contains('+'))// 仅SV支持批量，其他偷懒还没写
            {
               // if (typeof(TP) != typeof(PK9) && typeof(TP) != typeof(PA8)) return;//全版本后即可删除
                ProcessWithdraw(eventBody.MessageId);
                if (!VipRole && !BatchRole)
                    DodoBot<TP>.SendChannelMessage("你没有批量权限", eventBody.ChannelId);
                else
                {
                    if(VipRole)
                    {
                        DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                        new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiChinesePs(content.Trim(), eventBody.DodoSourceId,true,Count);
                        Count++;
                    }
                    else 
                        new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiChinesePs(content.Trim(), eventBody.DodoSourceId);
                }
                    return;
            }

            else if (content.Contains("取消"))
            {               
                var result = DodoBot<TP>.Info.ClearTrade(ulong.Parse(eventBody.DodoSourceId));
                DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $" {GetClearTradeMessage(result)}",
                    eventBody.ChannelId);
            }
            else if (content.Contains("位置"))
            {            
                var result = DodoBot<TP>.Info.CheckPosition(ulong.Parse(eventBody.DodoSourceId));
                DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId),
                    $" {GetQueueCheckResultMessage(result)}",
                    eventBody.ChannelId);
            }
            // var ps = ShowdownTranslator<TP>.Chinese2Showdown(content);
            var  ps = content;
            ProcessWithdraw(eventBody.MessageId);
            if (!string.IsNullOrWhiteSpace(ps))
            {
                if (ps.Trim() == "取消" || ps.Trim() == "位置") return;
                LogUtil.LogInfo($"收到命令\n{ps}", LogIdentity);
                if (VipRole)
                {
                    DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                    //new DodoHelper<TP>.StartTradeChinesePS(ps, parameter, false, Count);
                    new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeChinesePs(ps, true, Count);
                    Count++;
                }
                else
                {
                    new DodoHelper<TP>(ulong.Parse(eventBody.DodoSourceId),eventBody.Personal.NickName,eventBody.ChannelId,eventBody.IslandSourceId).StartTradeChinesePs(ps);
                }
            }
            
            else
            {
                DodoBot<TP>.SendChannelMessage($"{Welcome}", eventBody.ChannelId);
            }
        }

        public string GetQueueCheckResultMessage(QueueCheckResult<TP> result)
        {
            if (!result.InQueue || result.Detail is null)
                return "你目前不在队列里";
            var msg = $"你在第***{result.Position}位***";
            var pk = result.Detail.Trade.TradeData;
            if (pk.Species != 0)
                msg += $"，交换宝可梦：***{ShowdownTranslator<TP>.GameStringsZh.Species[result.Detail.Trade.TradeData.Species]}***";
            return msg;
        }

        private static string GetClearTradeMessage(QueueResultRemove result)
        {
            return result switch
            {
                QueueResultRemove.CurrentlyProcessing => "你正在交换队列中",
                QueueResultRemove.CurrentlyProcessingRemoved => "正在从队列中删除",
                QueueResultRemove.Removed => "已从队列中删除",
                _ => "你已经不在队列里",
            };
        }

        private void ProcessWithdraw(string messageId)
        {
            if (_dodoSettings.WithdrawTradeMessage)
            {
                DodoBot<TP>.OpenApiService.SetChannelMessageWithdraw(new SetChannelMessageWithdrawInput() { MessageId = messageId }, true);
            }
        }

        //禁言
        public void MemberMuteAdd(string islandSourceId, string dodoSourceId, int duration, string reason = "")
        {
            DodoBot<TP>.OpenApiService.SetMemberMuteAdd(new SetMemberMuteAddInput() { IslandSourceId = islandSourceId, DodoSourceId = dodoSourceId, Duration = duration, Reason = reason }, true);
        }
        public override void MessageReactionEvent(
            EventSubjectOutput<EventSubjectDataBusiness<EventBodyMessageReaction>> input)
        {
            // Do nothing
        }
    }
}