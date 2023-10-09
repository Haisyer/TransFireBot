﻿using System;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class BotFactory9SV : BotFactory<PK9>
    {
        public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PK9> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                => new PokeTradeBotSV(Hub, cfg),

            PokeRoutineType.RemoteControl => new RemoteControlBotSV(cfg),
            PokeRoutineType.RaidBot => new RaidBotSV(cfg, Hub),
            PokeRoutineType.RotatingRaidBot => new RotatingRaidBotSV(cfg, Hub),

            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                => true,

            PokeRoutineType.RemoteControl => true,
            PokeRoutineType.RaidBot => true,
            PokeRoutineType.RotatingRaidBot => true,

            _ => false,
        };
    }
}
