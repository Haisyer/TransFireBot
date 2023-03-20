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
        [Description("训练家选择宝可梦太慢了")]
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

        // Recovery -- General Bot Failures.恢复——一般的机器人故障
        // Anything below here should be retried once if possible.如果可能，下面的代码都应该重试一次。
        RoutineCancel,
        [Description("异常连接")]
        ExceptionConnection,
        [Description("内部异常")]
        ExceptionInternal,
        [Description("重新启动")]
        RecoverStart,
        [Description("重新输入连接密码")]
        RecoverPostLinkCode,
        [Description("重新打开盒子")]
        RecoverOpenBox,
        [Description("重新返回初始界面")]
        RecoverReturnOverworld,
        [Description("重新进入宝可入口站")]
        RecoverEnterUnionRoom,
    }

    public static class PokeTradeResultExtensions
    {
        public static bool ShouldAttemptRetry(this PokeTradeResult t) => t >= PokeTradeResult.RoutineCancel;
    }
}
