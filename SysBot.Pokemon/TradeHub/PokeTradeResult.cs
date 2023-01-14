using System.ComponentModel;

namespace SysBot.Pokemon
{
    public enum PokeTradeResult
    {
        [Description("成功")]
        Success,

        // Trade Partner Failures
        [Description("没找到连接对象")]
        NoTrainerFound,
        [Description("训练家太慢了")]
        TrainerTooSlow,
        [Description("训练家离开了")]
        TrainerLeft,
        [Description("训练家取消交易太快")]
        TrainerOfferCanceledQuick,
        [Description("训练家请求错误")]
        TrainerRequestBad,
        [Description("非法交换")]
        IllegalTrade,
        [Description("可疑交换")]
        SuspiciousActivity,

        // Recovery -- General Bot Failures
        // Anything below here should be retried once if possible.
        RoutineCancel,
        ExceptionConnection,
        ExceptionInternal,
        RecoverStart,
        RecoverPostLinkCode,
        RecoverOpenBox,
        RecoverReturnOverworld,
        RecoverEnterUnionRoom,
    }

    public static class PokeTradeResultExtensions
    {
        public static bool ShouldAttemptRetry(this PokeTradeResult t) => t >= PokeTradeResult.RoutineCancel;
    }
}
