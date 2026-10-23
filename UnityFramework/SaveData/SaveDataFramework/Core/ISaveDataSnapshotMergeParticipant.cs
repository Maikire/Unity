using Newtonsoft.Json.Linq;

namespace SaveData
{
    /// <summary>
    /// 可选的模块快照合并策略
    /// </summary>
    public interface ISaveDataSnapshotMergeParticipant
    {
        /// <summary>
        /// 合并独立的历史与当前快照，返回独立结果
        /// </summary>
        JToken MergeSnapshot(JToken previous, JToken current);

    }
}
