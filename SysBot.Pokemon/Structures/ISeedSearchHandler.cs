using PKHeX.Core;

namespace SysBot.Pokemon
{
    public interface ISeedSearchHandler<T> where T : PKM, new()
    {
        void CalculateAndNotify(T pkm, PokeTradeDetail<T> detail, SeedCheckSettings settings, PokeRoutineExecutor<T> bot);
    }

    public class NoSeedSearchHandler<T> : ISeedSearchHandler<T> where T : PKM, new()
    {
        public void CalculateAndNotify(T pkm, PokeTradeDetail<T> detail, SeedCheckSettings settings, PokeRoutineExecutor<T> bot)
        {
            const string msg = "未找到SEED搜索implementation. " +
                               "请告知机器人管理员,需要提供Z3文件";
            detail.SendNotification(bot, msg);
        }
    }
}