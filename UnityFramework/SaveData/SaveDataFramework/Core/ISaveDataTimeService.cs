using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 可注入的非缩放计时服务，完成或失败均须结束任务，取消后必须及时释放等待
    /// </summary>
    public interface ISaveDataTimeService
    {
        /// <summary>
        /// 等待指定实际时长，必须响应取消，不能阻塞调用线程或修改游戏状态
        /// </summary>
        Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken);

    }
}
