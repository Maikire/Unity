using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 支持托管计时器的平台默认实现
    /// </summary>
    public sealed class SystemSaveDataTimeService : ISaveDataTimeService
    {
        /// <summary>
        /// 通过系统任务计时等待实际时长，取消时立即结束等待
        /// </summary>
        public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken) =>
            Task.Delay(duration, cancellationToken);

    }
}
