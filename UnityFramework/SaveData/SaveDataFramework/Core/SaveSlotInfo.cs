using System;
namespace SaveData
{
    /// <summary>
    /// 栏位界面元信息，坏档保留诊断状态且不恢复运行时
    /// </summary>
    public sealed class SaveSlotInfo
    {
        /// <summary>
        /// 字符串栏位 ID
        /// </summary>
        public string SlotId { get; internal set; }

        /// <summary>
        /// 项目 schema ID
        /// </summary>
        public string SchemaId { get; internal set; }

        /// <summary>
        /// 读取迁移后的版本
        /// </summary>
        public int Version { get; internal set; }

        /// <summary>
        /// 保存 UTC 时间
        /// </summary>
        public DateTime SavedAtUtc { get; internal set; }

        /// <summary>
        /// 备份存在但未验证有效性
        /// </summary>
        public bool HasBackup { get; internal set; }

        /// <summary>
        /// 正式文档是否可读取和迁移
        /// </summary>
        public bool IsValid => Error == null;

        /// <summary>
        /// 正式文档诊断错误
        /// </summary>
        public Exception Error { get; internal set; }

    }
}
