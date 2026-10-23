using System;
using System.IO;

namespace SaveData.Example
{
    /// <summary>
    /// 将运行时进度接入存档适配器
    /// </summary>
    public sealed class SaveDataExampleParticipant : SaveDataParticipant<SaveDataExampleProgress>
    {
        /// <summary>
        /// 文档中的稳定模块 ID
        /// </summary>
        public const string Id = "progress";

        private readonly SaveDataExampleRuntime runtime;

        /// <summary>
        /// 当前适配器的模块 ID
        /// </summary>
        public override string ParticipantId => Id;

        /// <summary>
        /// 游戏应先创建运行时，再注册适配器
        /// </summary>
        public SaveDataExampleParticipant(SaveDataExampleRuntime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        /// <summary>
        /// 每次采集创建独立 DTO，后续运行时变化不影响已采集数据
        /// </summary>
        public override SaveDataExampleProgress CaptureData()
        {
            return new SaveDataExampleProgress
            {
                Coins = runtime.Coins,
                CheckpointId = runtime.CheckpointId
            };
        }

        /// <summary>
        /// 完整覆盖新游戏的所有持久化字段
        /// </summary>
        public override SaveDataExampleProgress CreateDefaultData()
        {
            return new SaveDataExampleProgress
            {
                Coins = 0,
                CheckpointId = "start"
            };
        }

        /// <summary>
        /// 校验只读取 DTO
        /// </summary>
        public override void ValidateData(SaveDataExampleProgress snapshot)
        {
            if (snapshot == null || snapshot.Coins < 0 || string.IsNullOrWhiteSpace(snapshot.CheckpointId))
                throw new InvalidDataException("示例进度无效，金币不能为负数，检查点不能为空");
        }

        /// <summary>
        /// 逐项复制完整快照
        /// </summary>
        public override void RestoreData(SaveDataExampleProgress snapshot)
        {
            ValidateData(snapshot);
            runtime.Coins = snapshot.Coins;
            runtime.CheckpointId = snapshot.CheckpointId;
        }

    }
}
