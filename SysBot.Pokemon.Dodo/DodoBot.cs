using System.Threading;
using System.Threading.Tasks;
using DoDo.Open.Sdk.Models;
using System.Linq;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Personals;
using DoDo.Open.Sdk.Models.ChannelMessages;
using DoDo.Open.Sdk.Models.Islands;
using DoDo.Open.Sdk.Services;
using PKHeX.Core;

namespace SysBot.Pokemon.Dodo
{
    public class DodoBot<T> where T : PKM, new()
    {
        private static PokeTradeHub<T> Hub = default!;
        internal static TradeQueueInfo<T> Info => Hub.Queues.Info;

        public static OpenApiService OpenApiService = default!;

        private static DodoSettings Settings = default!;

        public DodoBot(DodoSettings settings, PokeTradeHub<T> hub)
        {
            Hub = hub;
            Settings = settings;
            //开放接口服务
            OpenApiService = new OpenApiService(new OpenApiOptions
            {
                BaseApi = settings.BaseApi,
                ClientId = settings.ClientId,
                Token = settings.Token
            });
            //事件处理服务，可自定义，只要继承EventProcessService抽象类即可
            var eventProcessService = new PokemonProcessService<T>(OpenApiService, settings);
            //开放事件服务
            var openEventService = new OpenEventService(OpenApiService, eventProcessService, new OpenEventOptions
            {
                IsReconnect = true,
                IsAsync = true
            });
            //接收事件消息
            Task.Run(async () =>
            {
                StartDistribution();
                await openEventService.ReceiveAsync();

            });
        }

        public void StartDistribution()
        {
            var channelId = Settings.ChannelId;
            if (string.IsNullOrWhiteSpace(channelId)) return;
            //SendChannelMessage("开始派送！", channelId);
            SendChannelMessageAll("开始派送！", channelId);
            var Msg = "";
            if (!DodoBot<T>.Info.Hub.Config.Legality.AllowUseFile)
            {
                Msg = $"本频道不允许上传文件";
            }
            else
            {
                Msg = "本频道可以上传文件";
            }
            Task.Delay(1_000).ConfigureAwait(false);


            if (typeof(T) == typeof(PK8))
            {
                SendChannelMessage($"当前版本为剑盾,{Msg}", channelId);
            }
            else if (typeof(T) == typeof(PB8))
            {
                SendChannelMessage($"当前版本为晶灿钻石明亮珍珠,{Msg}", channelId);
            }
            else if (typeof(T) == typeof(PA8))
            {
                SendChannelMessage($"当前版本为阿尔宙斯,{Msg}", channelId);
            }
            else if (typeof(T) == typeof(PK9))
            {
                SendChannelMessage($"当前版本为朱紫,{Msg}", channelId);
            }
        }

        public static void SendChannelMessage(string message, string channelId)
        {
            if (string.IsNullOrEmpty(message)) return;
            OpenApiService.SetChannelMessageSend(new SetChannelMessageSendInput<MessageBodyText>
            {
                ChannelId = channelId,
                MessageBody = new MessageBodyText
                {
                    Content = message
                }
            });
        }

        public static void SendChannelAtMessage(ulong atDodoId, string message, string channelId)
        {
            if (string.IsNullOrEmpty(message)) return;
            OpenApiService.SetChannelMessageSend(new SetChannelMessageSendInput<MessageBodyText>
            {
                ChannelId = channelId,
                MessageBody = new MessageBodyText
                {
                    Content = $"<@!{atDodoId}> {message}"
                }
            });
        }

        public static void SendChannelMessageAll(string message, string channelId)
        {
            if (string.IsNullOrEmpty(message)) return;
            OpenApiService.SetChannelMessageSend(new SetChannelMessageSendInput<MessageBodyText>
            {
                ChannelId = channelId,
                MessageBody = new MessageBodyText
                {
                    Content = $"<@online> {message}"
                }
            });
        }

        public static void SendPersonalMessage(string dodoId, string message, string islandSourceId = "")
        {
            if (string.IsNullOrEmpty(message)) return;
            if (string.IsNullOrWhiteSpace(islandSourceId))
            {
                islandSourceId = OpenApiService.GetIslandList(new GetIslandListInput()).FirstOrDefault().IslandSourceId ?? "";
            }
            OpenApiService.SetPersonalMessageSend(new SetPersonalMessageSendInput<MessageBodyText>
            {
                IslandSourceId = islandSourceId,
                DodoSourceId = dodoId,
                MessageBody = new MessageBodyText
                {
                    Content = message
                }
            });
        }
        public static void SendChannelMessagePicture(string url, string channelId)
        {
            if (string.IsNullOrEmpty(url)) return;
            OpenApiService.SetChannelMessageSend(new SetChannelMessageSendInput<MessageBodyPicture>
            {
                ChannelId = channelId,
                MessageBody = new MessageBodyPicture
                {
                    Url = url,
                    Width = 1280,
                    Height = 720,
                    IsOriginal = 1
                }
            });
        }
        public static void SendPersonalMessagePicture(string url, string dodoId, string islandSourceId = "")
        {
            if (string.IsNullOrEmpty(url)) return;
            if (string.IsNullOrWhiteSpace(islandSourceId))
            {
                islandSourceId = OpenApiService.GetIslandList(new GetIslandListInput()).FirstOrDefault().IslandSourceId ?? "";
            }
            OpenApiService.SetPersonalMessageSend(new SetPersonalMessageSendInput<MessageBodyPicture>
            {
                IslandSourceId = islandSourceId,
                DodoSourceId = dodoId,
                MessageBody = new MessageBodyPicture
                {
                    Url = url,
                    Width = 1280,
                    Height = 720,
                    IsOriginal = 1
                }
            });
        }

    }
}