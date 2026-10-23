using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 通过 Unity 主循环和实际运行时间计时，Web 玩家默认使用，不用线程池计时器
    /// </summary>
    public sealed class UnityMainLoopSaveDataTimeService : ISaveDataTimeService
    {
        /// <summary>
        /// 在 Unity 主线程安排非缩放等待，主循环暂停时在恢复后的首个更新中检查期限
        /// </summary>
        public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (duration == TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }

            return SaveDataMainLoopTimer.GetInstance().Schedule(duration, cancellationToken);
        }

    }
}
