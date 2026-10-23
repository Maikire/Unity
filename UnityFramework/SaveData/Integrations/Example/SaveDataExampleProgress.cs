using Newtonsoft.Json;

namespace SaveData.Example
{
    /// <summary>
    /// 只包含需要持久化的进度数据
    /// </summary>
    public sealed class SaveDataExampleProgress
    {
        /// <summary>
        /// 玩家金币
        /// </summary>
        [JsonProperty("coins", Required = Required.Always)]
        public int Coins;

        /// <summary>
        /// 使用稳定 ID 保存检查点
        /// </summary>
        [JsonProperty("checkpointId", Required = Required.Always)]
        public string CheckpointId = "start";
    }

    /// <summary>
    /// 模拟游戏中的运行时数据，与存档 DTO 分开持有
    /// </summary>
    public sealed class SaveDataExampleRuntime
    {
        /// <summary>
        /// 当前金币
        /// </summary>
        public int Coins { get; set; }

        /// <summary>
        /// 当前检查点
        /// </summary>
        public string CheckpointId { get; set; } = "start";
    }
}
