using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Modules;
using Mirai.Net.Utils.Scaffolds;
using PKHeX.Core;
using System.Linq;

namespace SysBot.Pokemon.QQ
{
    public class DetectionModule<T> : IModule where T : PKM, new()
    {
        public bool? IsEnable { get; set; } = true;
        public void Execute(MessageReceiverBase @base)
        {
            var receiver = @base.Concretize<GroupMessageReceiver>();
            var qq = receiver.Sender.Id;
            var nickName = receiver.Sender.Name;
            var groupId = receiver.Sender.Group.ToString();
            QQSettings settings = MiraiQQBot<T>.Settings;

            if (receiver.MessageChain.OfType<AtMessage>().All(x => x.Target != settings.QQ)) return;

            var text = receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text ?? "";
            if (string.IsNullOrWhiteSpace(text)) return;

            else if (text.Trim().StartsWith("检测"))
            {
                new MiraiQQHelper<T>(qq, nickName).StartDump();
                return;
            }
        }
    }
}