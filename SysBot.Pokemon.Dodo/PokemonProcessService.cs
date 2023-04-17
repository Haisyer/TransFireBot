﻿using System;
using System.IO;
using System.Net;
using DoDo.Open.Sdk.Models.Bots;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Personals;
using DoDo.Open.Sdk.Models.Members;
using DoDo.Open.Sdk.Services;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.Dodo
{
    public class PokemonProcessService<TP> : EventProcessService where TP : PKM, new()
    {
        private readonly OpenApiService _openApiService;
        private static readonly string LogIdentity = "DodoBot";
        private static readonly string Welcome = "宝可梦机器人为您服务\n中文指令请看在线文件:https://docs.qq.com/doc/DVWdQdXJPWllabm5t?&u=1c5a2618155548239a9563e9f22a57c0\n或者使用PS代码\n或者上传pk文件\n取消排队请输入:取消\n当前位置请输入:位置";
        private readonly string _channelId;
        private string _botDodoSourceId;
        private uint Count = 0;
        private DodoParameter parameter;
        public PokemonProcessService(OpenApiService openApiService, string channelId)
        {
            _openApiService = openApiService;
            _channelId = channelId;
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
            parameter = new DodoParameter()
            {
                channelId = _channelId,
                dodoId = eventBody.DodoSourceId,
                islandid = eventBody.IslandSourceId,
                nickName = eventBody.Personal.NickName,
            };
            if (Count>100)
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
            if (eventBody.MessageBody is MessageBodyFile messageBodyFile)
            {
                if (!DodoBot<TP>.Info.Hub.Config.Legality.AllowUseFile)
                {
                    DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.ChannelId), "本频道不允许上传文件", eventBody.ChannelId);
                    return;
                }
                else
                {
                    if (!FileTradeHelper<TP>.ValidFileSize(messageBodyFile.Size ?? 0) || !FileTradeHelper<TP>.ValidFileName(messageBodyFile.Name))
                    {
                        DodoBot<TP>.SendChannelMessage("非法文件", eventBody.ChannelId);
                        return;
                    }
                    using var client = new HttpClient();
                    var downloadBytes = client.GetByteArrayAsync(messageBodyFile.Url).Result;
                    var pkms = FileTradeHelper<TP>.Bin2List(downloadBytes);
                    if (pkms.Count == 1)
                    {
                        if (VipRole)
                        {
                            DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                            DodoHelper<TP>.StartTrade(pkms[0], parameter, false, Count);
                            Count++;
                        }
                        else
                            DodoHelper<TP>.StartTrade(pkms[0], parameter);
                    }
                    else if (pkms.Count > 1 && pkms.Count <= FileTradeHelper<TP>.MaxCountInBin)
                    {
                        if (!BatchRole)
                        {
                            DodoBot<TP>.SendChannelMessage($"你不是VIP不能批量！", eventBody.ChannelId);
                            return;
                        }
                        string userpath = DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder + @"\" + eventBody.DodoSourceId;
                        if (!JudgeMultiNum(pkms.Count,parameter)) return;
                        DodoHelper<TP>.StartTradeMultiPKM(pkms, parameter, userpath, false, Count);
                    }
                    else
                        DodoBot<TP>.SendChannelMessage("文件内容不正确", eventBody.ChannelId);
                    return;
                }
            }

            if (eventBody.MessageBody is not MessageBodyText messageBodyText) return;

            var content = messageBodyText.Content;

            LogUtil.LogInfo($"{eventBody.Personal.NickName}({eventBody.DodoSourceId}):{content}", LogIdentity);
            if (_botDodoSourceId == null)
            {
                _botDodoSourceId = _openApiService.GetBotInfo(new GetBotInfoInput()).DodoSourceId;
            }
            if (!content.Contains($"<@!{_botDodoSourceId}>")) return;

            content = content.Substring(content.IndexOf('>') + 1);
            if (content.Trim().StartsWith("trade"))
            {
                content = content.Replace("trade", "");
                if (VipRole)
                {
                    DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                    DodoHelper<TP>.StartTrade(content, parameter, false,Count);
                    Count++;
                }
                else
                {
                    DodoHelper<TP>.StartTrade(content, parameter);
                    
                }
                return;
            } 
            else if (content.Trim().StartsWith("检测"))
            {
                DodoHelper<TP>.StartDump(parameter);
                return;
            }
            else if (content.Trim().StartsWith("队伍"))
            {
                var Mutipss = Regex.Split(content.Replace("队伍", ""), "\n\n"); ;
                if (Mutipss.Length > 1)
                {
                    if (!BatchRole)
                    {
                        DodoBot<TP>.SendChannelMessage($"你不是VIP不能批量！", eventBody.ChannelId);
                        return;
                    }
                    int legalNumber = 0;
                    int illegalNumber = 0;
                    var subpath1 = eventBody.DodoSourceId;
                    var userpath1 = DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder + @"\" + subpath1;
                    Directory.CreateDirectory(DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder + @"\" + subpath1);
                    foreach (var p in Mutipss)
                    {
                        legalNumber++;
                        LogUtil.LogInfo($"收到命令\n{p}", LogIdentity);
                        if (DodoHelper<TP>.CheckAndGetPkm(p, subpath1, out var msg, out var pk, out var id))
                        {
                            File.WriteAllBytes(userpath1 + @"\" + $"第{legalNumber}只.pk9", pk.Data);
                            LogUtil.LogInfo(msg, LogIdentity);
                        }
                        else
                        {
                            DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $"第{legalNumber}只非法", eventBody.ChannelId);
                            illegalNumber++;
                        }
                        if (legalNumber > DodoBot<TP>.Info.Hub.Config.Queues.MutiMaxNumber)
                        {
                            DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $"超出数量限制{DodoBot<TP>.Info.Hub.Config.Queues.MutiMaxNumber}", eventBody.ChannelId);
                            return;
                        }
                    }
                    if (legalNumber == illegalNumber)
                    {
                        DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $"全非法,换个屁", eventBody.ChannelId);
                        return;
                    }
                    DodoHelper<TP>.StartMutiTrade(parameter, subpath1, true);
                    content = null;
                }
            }
            else if (content.Trim().StartsWith("批量"))
            {
                if(!BatchRole)
                {
                    DodoBot<TP>.SendChannelMessage($"你不是VIP不能批量！", eventBody.ChannelId);
                    return;
                }
                if (DodoBot<TP>.Info.Hub.Config.Queues.MutiTrade)
                {
                    var r = content.Split('量');
                    if (r.Length > 0)
                    {
                        string directory = Path.Combine(DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder, r[1]);
                        string[] fileEntries = Directory.GetFiles(directory);
                        if (fileEntries.Length > 0)
                        {
                            DodoBot<TP>.SendChannelMessage($"找到{r[1]}", eventBody.ChannelId);
                            DodoHelper<TP>.StartMutiTrade(parameter, r[1],false);
                        }
                        else
                        {
                            DodoBot<TP>.SendChannelMessage($"未找到{r[1]}", eventBody.ChannelId);
                        }
                    }

                }
                return;
            }
            //截图测试用
            #region
            /*  else if (content.Trim().StartsWith("截图"))
              {
                  using (HttpClient client =new HttpClient())
                  {
                      client.DefaultRequestHeaders.Add("Authorization", "Bot 69804372.Njk4MDQzNzI.77-9OW_vv70.qvJQfqTiyAXPJlZx1THOL8hp2H3MjISyFpficc6OOOM");
                      MultipartFormDataContent contentFormData = new MultipartFormDataContent();
                      string path = DodoBot<TP>.Info.Hub.Config.Folder.ScreenshotFolder;
                      //添加文件参数，参数名为files，文件名为123.png
                      contentFormData.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(path)), "file", "image.jpg");
                      var requestUri = @"https://botopen.imdodo.com/api/v2/resource/picture/upload";
                      var result = client.PostAsync(requestUri, contentFormData).Result.Content.ReadAsStringAsync().Result;
                      LogUtil.LogInfo(result, LogIdentity);
                      DodoBot<TP>.SendChannelMessagePicture(GetDodoURL(), eventBody.ChannelId);
                  }
                  return;
              }
              private static string GetDodoURL()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", DodoBot<TP>.Info.Hub.Config.Dodo.DodoUploadFileUrl);
                MultipartFormDataContent contentFormData = new MultipartFormDataContent();
                string path = DodoBot<TP>.Info.Hub.Config.Folder.ScreenshotFolder;
                contentFormData.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(path)), "file", "a.jpg");
                var requestUri = @"https://botopen.imdodo.com/api/v2/resource/picture/upload";
                var result = client.PostAsync(requestUri, contentFormData).Result.Content.ReadAsStringAsync().Result;
                var a = result.Split("https");
                var b = a[1].Split("jpg");
                var c = "https" + b[0] + "jpg";
                return c;
            }

        }*/
            #endregion
            //   LogUtil.LogInfo(content, LogIdentity);
            var Mutips = Regex.Split(content, "[+]+"); ;
            if (Mutips.Length > 1) 
            {
                if (!BatchRole)
                {
                    DodoBot<TP>.SendChannelMessage($"你不是VIP不能批量！", eventBody.ChannelId);
                    return;
                }
                int i = 0;
                int j = 0;
                var subpath = eventBody.DodoSourceId;
                var userpath = DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder + @"\" + subpath;
                Directory.CreateDirectory(DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder+@"\"+ subpath);
                foreach (var p in Mutips)
                {
                    i++;
                    var pss = ShowdownTranslator<TP>.Chinese2Showdown(p);
                    LogUtil.LogInfo($"收到命令\n{pss}\n", LogIdentity);
                    if (DodoHelper<TP>.CheckAndGetPkm(pss, subpath, out var msg, out var pk, out var id))
                    {
                        File.WriteAllBytes(userpath + @"\" + $"第{i}只.pk9", pk.Data);
                        LogUtil.LogInfo(msg, LogIdentity);
                    }
                    else
                    {
                        DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $"第{i}只非法", eventBody.ChannelId);
                        j++;
                    }
                    if(i> DodoBot<TP>.Info.Hub.Config.Queues.MutiMaxNumber)
                    {
                        DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $"超出数量限制{DodoBot<TP>.Info.Hub.Config.Queues.MutiMaxNumber}",eventBody.ChannelId);
                        return;
                    }
                }
                if(i==j)
                {
                    DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $"全非法,换个屁", eventBody.ChannelId);
                    return;
                }
                DodoHelper<TP>.StartMutiTrade(parameter, subpath, true);
                content = null;
            }
            var ps = ShowdownTranslator<TP>.Chinese2Showdown(content);
            if (!string.IsNullOrWhiteSpace(ps))
            {
                LogUtil.LogInfo($"收到命令\n{ps}", LogIdentity);
                if (VipRole)
                {
                    DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                    DodoHelper<TP>.StartTrade(ps, parameter, false, Count);
                    Count++;
                }
                else
                {
                    DodoHelper<TP>.StartTrade(ps, parameter);
                }
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
        private bool JudgeMultiNum(int multiNum, DodoParameter p)
        {
            var maxPkmsPerTrade = DodoBot<TP>.Info.Hub.Config.Queues.MutiMaxNumber;
            if (maxPkmsPerTrade <= 1)
            {
                DodoBot<TP>.SendChannelAtMessage(ulong.Parse(p.dodoId), "批量交换你把握不住的，洗洗睡吧", p.channelId);
                return false;
            }
            else if (multiNum > maxPkmsPerTrade)
            {
                DodoBot<TP>.SendChannelAtMessage(ulong.Parse(p.dodoId), $"批量交换宝可梦数量应小于等于{maxPkmsPerTrade}", p.channelId);
                return false;
            }
            return true;
        }


        public override void MessageReactionEvent(
            EventSubjectOutput<EventSubjectDataBusiness<EventBodyMessageReaction>> input)
        {
            // Do nothing
        }
    }
}