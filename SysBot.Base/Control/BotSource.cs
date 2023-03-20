using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Base
{
    public class BotSource<T> where T : class, IConsoleBotConfig
    {
        public readonly RoutineExecutor<T> Bot;
        private CancellationTokenSource Source = new();

        public BotSource(RoutineExecutor<T> bot) => Bot = bot;

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        private bool IsStopping { get; set; }

        public void Stop()
        {
            if (!IsRunning || IsStopping)
                return;

            IsStopping = true;
            Source.Cancel();
            Source = new CancellationTokenSource();

            Task.Run(async () =>
            {
                await Bot.HardStop().ConfigureAwait(false);
                IsPaused = IsRunning = IsStopping = false;
            });
        }

        public void Pause()
        {
            if (!IsRunning || IsStopping)
                return;

            IsPaused = true;
            Bot.SoftStop();
        }

        public void Start()
        {
            if (IsPaused)
                Stop(); // can't soft-resume; just re-launch

            if (IsRunning || IsStopping)
                return;

            Task.Run(() => Bot.RunAsync(Source.Token)
                .ContinueWith(ReportFailure, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ => IsRunning = false));

            IsRunning = true;
        }

        private void ReportFailure(Task finishedTask)
        {
            var ident = Bot.Connection.Name;
            var ae = finishedTask.Exception;
            if (ae == null)
            {
                LogUtil.LogError("机器人已经正常停止。", ident);
                return;
            }

            LogUtil.LogError("机器人崩溃了!", ident);

            if (!string.IsNullOrEmpty(ae.Message))
                LogUtil.LogError("全部信息: " + ae.Message, ident);

            var st = ae.StackTrace;
            if (!string.IsNullOrEmpty(st))
                LogUtil.LogError("Aggregate stacktrace: " + st, ident);

            foreach (var e in ae.InnerExceptions)
            {
                if (!string.IsNullOrEmpty(e.Message))
                    LogUtil.LogError("内部消息: " + e.Message, ident);
                LogUtil.LogError("Inner stacktrace: " + e.StackTrace, ident);
            }
        }

        public void Resume()
        {
            Start();
        }
    }
}
