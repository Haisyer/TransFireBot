using PKHeX.Core;
using SysBot.Base;
using System;
using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon
{
    public class StreamSettings
    {
        private const string Operation = nameof(Operation);

        public override string ToString() => "数据流设置";
        public static Action<PKM, string>? CreateSpriteFile { get; set; }

        [Category(Operation), Description("生成数据流资源；关闭将阻止资源的生成。")]
        public bool CreateAssets { get; set; } = false;

        [Category(Operation), Description("生成交易开始细节，显示机器人正在与谁交易。")]
        public bool CreateTradeStart { get; set; } = true;

        [Category(Operation), Description("生成交易开始细节，显示机器人正在交易什么。")]
        public bool CreateTradeStartSprite { get; set; } = true;

        [Category(Operation), Description("显示当前交易的详细信息格式。{0} = ID, {1} = 用户")]
        public string TrainerTradeStart { get; set; } = "(ID {0}) {1}";

        // On Deck

        [Category(Operation), Description("生成当前on-deck的人员列表。")]
        public bool CreateOnDeck { get; set; } = true;

        [Category(Operation), Description("on-deck 列表中显示的用户数量。")]
        public int OnDeckTake { get; set; } = 5;

        [Category(Operation), Description("on-deck上可跳过的用户数量。如果您想隐藏正在处理的人员，请将此设置为您的控制台数量。")]
        public int OnDeckSkip { get; set; } = 0;

        [Category(Operation), Description("分隔符用于分隔on-deck上的列表用户。")]
        public string OnDeckSeparator { get; set; } = "\n";

        [Category(Operation), Description("显示on-deck上的列表用户格式。{0} = ID, {3} = 用户")]
        public string OnDeckFormat { get; set; } = "(ID {0}) - {3}";

        // On Deck 2

        [Category(Operation), Description("生成当前on-deck #2上的人员列表。")]
        public bool CreateOnDeck2 { get; set; } = true;

        [Category(Operation), Description("on-deck #2列表中显示的用户数量。")]
        public int OnDeckTake2 { get; set; } = 5;

        [Category(Operation), Description("on-deck #2上可跳过的用户数量. 如果您想隐藏正在处理的人员，请将此设置为您的控制台数量。")]
        public int OnDeckSkip2 { get; set; } = 0;

        [Category(Operation), Description("分隔符用于分隔on-deck #2上的列表用户。")]
        public string OnDeckSeparator2 { get; set; } = "\n";

        [Category(Operation), Description("显示on-deck #2上的列表用户格式. {0} = ID, {3} = 用户")]
        public string OnDeckFormat2 { get; set; } = "(ID {0}) - {3}";

        // Raid On Deck

        [Category(Operation), Description("Generate a list of People currently on-deck.")]
        public bool CreateRaidOnDeck { get; set; } = true;

        [Category(Operation), Description("Number of raids to show in the raid on-deck list.")]
        public int RaidOnDeckTake { get; set; } = 5;

        [Category(Operation), Description("Number of raids on-deck to skip at the top. If you want to hide people being processed, set this to your number of consoles.")]
        public int RaidOnDeckSkip { get; set; }

        [Category(Operation), Description("Separator to split the on-deck list users.")]
        public string RaidOnDeckSeparator { get; set; } = "\n";

        [Category(Operation), Description("Format to display the Raid Info. {0} = Count")]
        public string RaidInfoFormat { get; set; } = "Raid Info: {0}";

        [Category(Operation), Description("Format to display the Raid Rewards. {0} = Count")]
        public string RaidRewardsInQueueFormat { get; set; } = "{0}";

        [Category(Operation), Description("Format to display the Raid Moveset. {0} = Count")]
        public string RaidMovesetFormat { get; set; } = "Moveset: {0}";

        // User List

        [Category(Operation), Description("生成当前正在交易的人员列表。")]
        public bool CreateUserList { get; set; } = true;

        [Category(Operation), Description("用户数量显示在列表中。")]
        public int UserListTake { get; set; } = -1;

        [Category(Operation), Description("可跳过的用户数量。如果您想隐藏正在处理的人员，请将此设置为您的控制台数量。")]
        public int UserListSkip { get; set; } = 0;

        [Category(Operation), Description("分隔符分隔列表用户。")]
        public string UserListSeparator { get; set; } = ", ";

        [Category(Operation), Description("显示列表用户格式。{0} = ID, {3} = 用户")]
        public string UserListFormat { get; set; } = "(ID {0}) - {3}";

        // TradeCodeBlock

        [Category(Operation), Description("如果TradeBlockFile存在，则复制它，否则复制一个占位符图像。")]
        public bool CopyImageFile { get; set; } = true;

        [Category(Operation), Description("输入连接密码时要复制的图像的源文件名。如果为空，将创建一个占位符图像。")]
        public string TradeBlockFile { get; set; } = string.Empty;

        [Category(Operation), Description("“连接密码”块映像的目标文件名。{0} 被替换为本地IP地址。")]
        public string TradeBlockFormat { get; set; } = "block_{0}.png";

        // Waited Time

        [Category(Operation), Description("创建一个文件，列出最近退出队列的用户等待的时间。")]
        public bool CreateWaitedTime { get; set; } = true;

        [Category(Operation), Description("显示最近退出队列的用户的等待时间格式。")]
        public string WaitedTimeFormat { get; set; } = @"hh\:mm\:ss";

        // Estimated Time

        [Category(Operation), Description("创建一个文件，列出用户加入队列后预计需要等待的时间。")]
        public bool CreateEstimatedTime { get; set; } = true;

        [Category(Operation), Description("显示大概预计时间的消息格式。")]
        public string EstimatedTimeFormat { get; set; } = "预计等待时间: {0:F1} 分钟";

        [Category(Operation), Description("显示预计等待时间的格式。")]
        public string EstimatedFulfillmentFormat { get; set; } = @"hh\:mm\:ss";

        // Users in Queue

        [Category(Operation), Description("创建一个文件，显示队列中的用户数量。")]
        public bool CreateUsersInQueue { get; set; } = true;

        [Category(Operation), Description("显示队列中的用户的格式。{0} = 数量")]
        public string UsersInQueueFormat { get; set; } = "排队用户: {0}";

        // Completed Trades

        [Category(Operation), Description("当新的交易开始时，创建一个文件来显示完成的交易数量。")]
        public bool CreateCompletedTrades { get; set; } = true;

        [Category(Operation), Description("显示已完成交易的格式。{0} = 数量")]
        public string CompletedTradesFormat { get; set; } = "已完成交易: {0}";

        public void StartTrade<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail, PokeTradeHub<T> hub) where T : PKM, new()
        {
            if (!CreateAssets)
                return;

            try
            {
                if (CreateTradeStart)
                    GenerateBotConnection(b, detail);
                if (CreateWaitedTime)
                    GenerateWaitedTime(detail.Time);
                if (CreateEstimatedTime)
                    GenerateEstimatedTime(hub);
                if (CreateUsersInQueue)
                    GenerateUsersInQueue(hub.Queues.Info.Count);
                if (CreateOnDeck)
                    GenerateOnDeck(hub);
                if (CreateOnDeck2)
                    GenerateOnDeck2(hub);
                if (CreateUserList)
                    GenerateUserList(hub);
                if (CreateCompletedTrades)
                    GenerateCompletedTrades(hub);
                if (CreateTradeStartSprite)
                    GenerateBotSprite(b, detail);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogError(e.Message, nameof(StreamSettings));
            }
        }

        public void IdleAssets(PokeRoutineExecutorBase b)
        {
            if (!CreateAssets)
                return;

            try
            {
                var files = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (file.Contains(b.Connection.Name))
                        File.Delete(file);
                }

                if (CreateWaitedTime)
                    File.WriteAllText("waited.txt", "00:00:00");
                if (CreateEstimatedTime)
                {
                    File.WriteAllText("estimatedTime.txt", "Estimated time: 0 minutes");
                    File.WriteAllText("estimatedTimestamp.txt", "");
                }
                if (CreateOnDeck)
                    File.WriteAllText("ondeck.txt", "Waiting...");
                if (CreateOnDeck2)
                    File.WriteAllText("ondeck2.txt", "Queue is empty!");
                if (CreateUserList)
                    File.WriteAllText("users.txt", "None");
                if (CreateUsersInQueue)
                    File.WriteAllText("queuecount.txt", "Users in Queue: 0");
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogError(e.Message, nameof(StreamSettings));
            }
        }

        private void GenerateUsersInQueue(int count)
        {
            var value = string.Format(UsersInQueueFormat, count);
            File.WriteAllText("queuecount.txt", value);
        }

        private void GenerateWaitedTime(DateTime time)
        {
            var now = DateTime.Now;
            var difference = now - time;
            var value = difference.ToString(WaitedTimeFormat);
            File.WriteAllText("waited.txt", value);
        }

        private void GenerateEstimatedTime<T>(PokeTradeHub<T> hub) where T : PKM, new()
        {
            var count = hub.Queues.Info.Count;
            var estimate = hub.Config.Queues.EstimateDelay(count, hub.Bots.Count);

            // Minutes
            var wait = string.Format(EstimatedTimeFormat, estimate);
            File.WriteAllText("estimatedTime.txt", wait);

            // Expected to be fulfilled at this time
            var now = DateTime.Now;
            var difference = now.AddMinutes(estimate);
            var date = difference.ToString(EstimatedFulfillmentFormat);
            File.WriteAllText("estimatedTimestamp.txt", date);
        }

        public void StartEnterCode(PokeRoutineExecutorBase b)
        {
            if (!CreateAssets)
                return;

            try
            {
                var file = GetBlockFileName(b);
                if (CopyImageFile && File.Exists(TradeBlockFile))
                    File.Copy(TradeBlockFile, file);
                else
                    File.WriteAllBytes(file, BlackPixel);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogError(e.Message, nameof(StreamSettings));
            }
        }

        private static readonly byte[] BlackPixel = // 1x1 black pixel
        {
            0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00,
        };

        public void EndEnterCode(PokeRoutineExecutorBase b)
        {
            try
            {
                var file = GetBlockFileName(b);
                if (File.Exists(file))
                    File.Delete(file);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogError(e.Message, nameof(StreamSettings));
            }
        }

        private string GetBlockFileName(PokeRoutineExecutorBase b) => string.Format(TradeBlockFormat, b.Connection.Name);

        private void GenerateBotConnection<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail) where T : PKM, new()
        {
            var file = b.Connection.Name;
            var name = string.Format(TrainerTradeStart, detail.ID, detail.Trainer.TrainerName, (Species)detail.TradeData.Species);
            File.WriteAllText($"{file}.txt", name);
        }

        private static void GenerateBotSprite<T>(PokeRoutineExecutorBase b, PokeTradeDetail<T> detail) where T : PKM, new()
        {
            var func = CreateSpriteFile;
            if (func == null)
                return;
            var file = b.Connection.Name;
            var pk = detail.TradeData;
            func.Invoke(pk, $"sprite_{file}.png");
        }

        private void GenerateOnDeck<T>(PokeTradeHub<T> hub) where T : PKM, new()
        {
            var ondeck = hub.Queues.Info.GetUserList(OnDeckFormat);
            ondeck = ondeck.Skip(OnDeckSkip).Take(OnDeckTake); // filter down
            File.WriteAllText("ondeck.txt", string.Join(OnDeckSeparator, ondeck));
        }

        private void GenerateOnDeck2<T>(PokeTradeHub<T> hub) where T : PKM, new()
        {
            var ondeck = hub.Queues.Info.GetUserList(OnDeckFormat2);
            ondeck = ondeck.Skip(OnDeckSkip2).Take(OnDeckTake2); // filter down
            File.WriteAllText("ondeck2.txt", string.Join(OnDeckSeparator2, ondeck));
        }

        private void GenerateUserList<T>(PokeTradeHub<T> hub) where T : PKM, new()
        {
            var users = hub.Queues.Info.GetUserList(UserListFormat);
            users = users.Skip(UserListSkip);
            if (UserListTake > 0)
                users = users.Take(UserListTake); // filter down
            File.WriteAllText("users.txt", string.Join(UserListSeparator, users));
        }

        private void GenerateCompletedTrades<T>(PokeTradeHub<T> hub) where T : PKM, new()
        {
            var msg = string.Format(CompletedTradesFormat, hub.Config.Trade.CompletedTrades);
            File.WriteAllText("completed.txt", msg);
        }

        public async Task StartRaid(PokeRoutineExecutorBase b, PK9 pk, PK9 pknext, int i, PokeTradeHub<PK9> hub, int type, CancellationToken token)
        {
            if (!CreateAssets)
                return;

            try
            {
                if (CreateRaidOnDeck)
                {
                    await GenerateRaidInfo(i, hub, type, token).ConfigureAwait(false);
                }
                if (CreateTradeStartSprite)
                {
                    await GenerateRaidBotSprite(b, pk, token).ConfigureAwait(false);
                    if ((Species)pknext.Species != Species.None)
                        await GenerateNextRaidBotSprite(b, pknext, token).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                LogUtil.LogError(e.Message, nameof(StreamSettings));
            }
        }

        private async Task GenerateRaidInfo(int i, PokeTradeHub<PK9> hub, int type, CancellationToken token)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            var info = string.Empty;
            var title = string.Empty;
            string[] description = Array.Empty<string>();
            switch (type)
            {
                case 0: title = hub.Config.RaidSV.RaidEmbedFilters.Title; info = hub.Config.RaidSV.RaidEmbedFilters.Description[1]; description = hub.Config.RaidSV.RaidEmbedFilters.Description; break;
                case 1: title = hub.Config.RotatingRaidSV.RaidEmbedParameters[i].Title; info = hub.Config.RotatingRaidSV.RaidEmbedParameters[i].Description[1]; description = hub.Config.RotatingRaidSV.RaidEmbedParameters[i].Description; break;
            }
            var titleval = string.Format(RaidInfoFormat, title);
            if (!string.IsNullOrEmpty(titleval))
                File.WriteAllText("raidtitle.txt", titleval);
            else
                File.WriteAllText("raidtitle.txt", "No raid title found.");

            var infoval = string.Format(RaidInfoFormat, info);
            if (!string.IsNullOrEmpty(infoval))
                File.WriteAllText("raidinfo.txt", infoval);
            else
                File.WriteAllText("raidinfo.txt", "No raid info found.");

            if (description.Length > 0)
            {
                string[] moves = description.ToString()!.Split("**Moveset**");
                string value = string.Format(RaidMovesetFormat, string.Join(Environment.NewLine, moves[1]).Trim());
                if (!string.IsNullOrEmpty(value))
                    File.WriteAllText("raidmoveset.txt", value);
                else
                    File.WriteAllText("raidmoveset.txt", "No moveset found.");

                string[] rewards = moves.ToString()!.Split("**Special Rewards**");
                var rewardvalue = string.Format(RaidRewardsInQueueFormat, string.Join(Environment.NewLine, rewards[1]).Trim());
                if (!string.IsNullOrEmpty(rewardvalue))
                    File.WriteAllText("raidrewards.txt", rewardvalue);
                else
                    File.WriteAllText("raidrewards.txt", "No special rewards found.");
            }
        }

        private static async Task GenerateRaidBotSprite(PokeRoutineExecutorBase b, PK9 pk, CancellationToken token)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            var func = CreateSpriteFile;
            if (func == null)
                return;
            var file = b.Connection.Name;
            func.Invoke(pk, $"sprite_{file}.png");
        }

        private static async Task GenerateNextRaidBotSprite(PokeRoutineExecutorBase b, PK9 pk, CancellationToken token)
        {
            await Task.Delay(0_100, token).ConfigureAwait(false);
            var func = CreateSpriteFile;
            if (func == null)
                return;
            var file = b.Connection.Name;
            func.Invoke(pk, $"nextsprite_{file}.png");
        }
    }
}