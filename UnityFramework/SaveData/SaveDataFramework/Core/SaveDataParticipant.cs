using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 强类型 DTO 适配器，每次回调使用独立对象
    /// </summary>
    public abstract class SaveDataParticipant<T> : ISaveDataParticipant, ISaveDataSnapshotMergeParticipant where T : class
    {
        /// <summary>
        /// 稳定的字符串模块 ID
        /// </summary>
        public abstract string ParticipantId { get; }

        /// <summary>
        /// 默认恢复优先级
        /// </summary>
        public virtual int RestoreOrder => 0;

        /// <summary>
        /// 默认没有模块依赖
        /// </summary>
        public virtual IReadOnlyCollection<string> Dependencies => Array.Empty<string>();

        /// <summary>
        /// 检查项目资源和角色就绪状态
        /// </summary>
        public virtual void EnsureReady()
        {
        }

        /// <summary>
        /// 默认同步检查，异步资源模块应覆盖此方法
        /// </summary>
        public virtual Task WaitUntilReadyAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureReady();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 复制运行时状态为 DTO
        /// </summary>
        public abstract T CaptureData();

        /// <summary>
        /// 创建覆盖全部持久化状态的新游戏默认 DTO
        /// </summary>
        public abstract T CreateDefaultData();

        /// <summary>
        /// 校验 DTO
        /// </summary>
        public abstract void ValidateData(T snapshot);

        /// <summary>
        /// 校验采集的 DTO，项目可单独覆盖以区别保存与恢复前置条件
        /// </summary>
        public virtual void ValidateCaptureData(T snapshot) => ValidateData(snapshot);

        /// <summary>
        /// 将 DTO 复制到运行时，必须支持此前采集的回退快照
        /// </summary>
        public abstract void RestoreData(T snapshot);

        /// <summary>
        /// 编码独立运行时快照
        /// </summary>
        public JToken Capture()
        {
            T snapshot = CaptureData();
            ValidateCaptureData(snapshot);
            return SaveDataJson.ToToken(snapshot);
        }

        /// <summary>
        /// 编码独立默认快照
        /// </summary>
        public JToken CreateDefault() => SaveDataJson.ToToken(CreateDefaultData());

        /// <summary>
        /// 解码独立 DTO 后校验
        /// </summary>
        public void ValidateRestore(JToken snapshot) => ValidateData(SaveDataJson.FromToken<T>(snapshot));

        /// <summary>
        /// 解码独立 DTO 后恢复
        /// </summary>
        public void Restore(JToken snapshot) => RestoreData(SaveDataJson.FromToken<T>(snapshot));

        /// <summary>
        /// 按 DTO 契约合并未知字段，项目可覆盖以定义自己的完整快照策略
        /// </summary>
        public virtual JToken MergeSnapshot(JToken previous, JToken current) =>
            SaveDataJson.MergeSnapshot(previous, current, typeof(T));

    }
}
