using PKHeX.Core;
using SysBot.Pokemon;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQQueue<T> where T : PKM, new()
    {
        public T Pokemon { get; }
        public PokeTradeTrainerInfo Trainer { get; }
        public ulong QQ { get; }
        public string DisplayName => Trainer.TrainerName;

        public MiraiQQQueue(T pkm, PokeTradeTrainerInfo trainer, ulong qq)
        {
            Pokemon = pkm;
            Trainer = trainer;
            QQ = qq;
        }
    }
}