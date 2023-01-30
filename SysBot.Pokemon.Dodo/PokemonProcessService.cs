using System;
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
                    if (!ValidFileSize(messageBodyFile.Size ?? 0) || !ValidFileName(messageBodyFile.Name))
                    {
                        DodoBot<TP>.SendChannelMessage("非法文件", eventBody.ChannelId);
                        return;
                    }
                    using var client = new HttpClient();
                    var downloadBytes = client.GetByteArrayAsync(messageBodyFile.Url).Result;
                    var p = GetPKM(downloadBytes);
                    if (p is TP pkm)
                    {
                        if (VipRole)
                        {
                            DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                            DodoHelper<TP>.StartTrade(pkm, eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId,eventBody.IslandSourceId, false, Count);
                            Count++;
                        }
                        else
                            DodoHelper<TP>.StartTrade(pkm, eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId);
                    }

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
                    DodoHelper<TP>.StartTrade(content, eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId, false,Count);
                    Count++;
                }
                else
                {
                    DodoHelper<TP>.StartTrade(content, eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId);
                    
                }
                return;
            } 
            else if (content.Trim().StartsWith("检测"))
            {
                DodoHelper<TP>.StartDump(eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId);
                return;
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
                            DodoHelper<TP>.StartMutiTrade(eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId, r[1],false);
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
              }*/
            #endregion
         //   LogUtil.LogInfo(content, LogIdentity);
            var Mutips = Regex.Split(content, "[\r\n]+"); ;
            if (Mutips.Length > 1) 
            {
                if (!BatchRole)
                {
                    DodoBot<TP>.SendChannelMessage($"你不是VIP不能批量！", eventBody.ChannelId);
                    return;
                }
                int i = 0;
                var userpath = DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder + @"\" + eventBody.DodoSourceId;
                Directory.CreateDirectory(DodoBot<TP>.Info.Hub.Config.Folder.TradeFolder+@"\"+ eventBody.DodoSourceId);
                foreach (var p in Mutips)
                {
                    i++;
                    var pss = ShowdownTranslator<TP>.Chinese2Showdown(p);
                    LogUtil.LogInfo($"收到命令\n{pss}\n", LogIdentity);
                    DodoHelper<TP>.GetPkm(pss, eventBody.DodoSourceId, out var msg, out var pk, out var id);
                    File.WriteAllBytes(userpath +@"\"+$"第{i}只.pk9", pk.Data);
                    LogUtil.LogInfo(msg, LogIdentity);
                }
                DodoHelper<TP>.StartMutiTrade(eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId, eventBody.DodoSourceId,true);
                content = null;
            }
            var ps = ShowdownTranslator<TP>.Chinese2Showdown(content);
            if (!string.IsNullOrWhiteSpace(ps))
            {
                LogUtil.LogInfo($"收到命令\n{ps}", LogIdentity);
                if (VipRole)
                {
                    DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), "尊贵的VIP用户,请走VIP通道", eventBody.ChannelId);
                    DodoHelper<TP>.StartTrade(ps, eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId, false, Count);
                    Count++;
                }
                else
                {
                    DodoHelper<TP>.StartTrade(ps, eventBody.DodoSourceId, eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId);
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

        private static bool ValidFileSize(long size)
        {
            if (typeof(TP) == typeof(PK8) || typeof(TP) == typeof(PB8) || typeof(TP) == typeof(PK9))
            {
                return size == 344;
            }

            if (typeof(TP) == typeof(PA8))
            {
                return size == 376;
            }

            return false;
        }

        private static bool ValidFileName(string fileName)
        {
            return (typeof(TP) == typeof(PK8) && fileName.EndsWith("pk8", StringComparison.OrdinalIgnoreCase)
                    || typeof(TP) == typeof(PB8) && fileName.EndsWith("pb8", StringComparison.OrdinalIgnoreCase)
                    || typeof(TP) == typeof(PA8) && fileName.EndsWith("pa8", StringComparison.OrdinalIgnoreCase)
                    || typeof(TP) == typeof(PK9) && fileName.EndsWith("pk9", StringComparison.OrdinalIgnoreCase));
        }

        private static PKM GetPKM(byte[] bytes)
        {
            if (typeof(TP) == typeof(PK8)) return new PK8(bytes);
            if (typeof(TP) == typeof(PB8)) return new PB8(bytes);
            if (typeof(TP) == typeof(PA8)) return new PA8(bytes);
            if (typeof(TP) == typeof(PK9)) return new PK9(bytes);
            return null;
        }

        public override void MessageReactionEvent(
            EventSubjectOutput<EventSubjectDataBusiness<EventBodyMessageReaction>> input)
        {
            // Do nothing
        }
    }
}