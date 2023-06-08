using PKHeX.Core;

namespace SysBot.Pokemon
{
    /// <summary>
    /// Stores data for indicating how a queue position/presence check resulted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed record QueueCheckResult<T> where T : PKM, new()
    {
        public readonly bool InQueue;
        public readonly TradeEntry<T>? Detail;
        public readonly int Position;
        public readonly int QueueCount;

        public static readonly QueueCheckResult<T> None = new();

        public QueueCheckResult(bool inQueue = false, TradeEntry<T>? detail = default, int position = -1, int queueCount = -1)
        {
            InQueue = inQueue;
            Detail = detail;
            Position = position;
            QueueCount = queueCount;
        }

        public string GetMessage()
        {
            if (!InQueue || Detail is null)
                return "你不在队伍里。";
            var position = $"{Position}/{QueueCount}";
            var msg = $"你已经在 {Detail.Type} 队列中! 位置: {position} (ID {Detail.Trade.ID})";
            var pk = Detail.Trade.TradeData;
            if (pk.Species != 0)
                msg += $", 接收到: {GameInfo.GetStrings(1).Species[pk.Species]}";
            return msg;
        }
    }
}