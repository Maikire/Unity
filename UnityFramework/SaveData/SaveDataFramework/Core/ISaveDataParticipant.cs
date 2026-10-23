using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 模块只处理自己的快照，默认值覆盖全部持久化状态
    /// </summary>
    public interface ISaveDataParticipant
    {
        /// <summary>
        /// 稳定的字符串模块 ID，也是文档中的模块键
        /// </summary>
        string ParticipantId { get; }

        /// <summary>
        /// 无依赖约束时使用的恢复优先级
        /// </summary>
        int RestoreOrder { get; }

        /// <summary>
        /// 必须先恢复的模块 ID，依赖必须已经注册
        /// </summary>
        IReadOnlyCollection<string> Dependencies { get; }

        /// <summary>
        /// 同步检查就绪状态
        /// </summary>
        void EnsureReady();

        /// <summary>
        /// 等待项目准备好模块，必须响应取消并结束任务，忽略取消会继续占用操作队列
        /// </summary>
        Task WaitUntilReadyAsync(CancellationToken cancellationToken);

        /// <summary>
        /// 复制运行时状态
        /// </summary>
        JToken Capture();

        /// <summary>
        /// 创建完整的新游戏默认快照
        /// </summary>
        JToken CreateDefault();

        /// <summary>
        /// 读取快照并检查恢复条件
        /// </summary>
        void ValidateRestore(JToken snapshot);

        /// <summary>
        /// 将快照复制到运行时，也用于失败后的回退
        /// </summary>
        void Restore(JToken snapshot);

    }
}
