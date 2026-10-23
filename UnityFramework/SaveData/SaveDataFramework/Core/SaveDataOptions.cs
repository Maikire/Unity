using System;
using System.Collections.Generic;

namespace SaveData
{
    /// <summary>
    /// 会话初始化配置，Manager 初始化时复制，初始化后不接受配置变更
    /// </summary>
    public sealed class SaveDataOptions
    {
        /// <summary>
        /// 项目 schema
        /// </summary>
        public ISaveDataSchema Schema { get; set; } = new SaveDataSchema("default");

        /// <summary>
        /// 可替换存储，空值使用 Unity 的本地持久化目录
        /// </summary>
        public ISaveDataStorage Storage { get; set; }

        /// <summary>
        /// 可替换内部逻辑编码器，空值使用 Newtonsoft JSON，再统一压缩、封装完整文件和整文件加密
        /// </summary>
        public ISaveDataSerializer Serializer { get; set; }

        /// <summary>
        /// 加密前的完整文件格式，自动模式按编辑器或开发构建及发布构建选择
        /// </summary>
        public SaveDataFileFormat FileFormat { get; set; } = SaveDataFileFormat.Automatic;

        /// <summary>
        /// 默认 JSON 编码器是否使用缩进
        /// </summary>
        public bool PrettyPrintJson { get; set; } = true;

        /// <summary>
        /// 内置整文件加密选项，默认不加密
        /// </summary>
        public SaveDataEncryptionMode EncryptionMode { get; set; } = SaveDataEncryptionMode.None;

        /// <summary>
        /// 可替换写入加密，提供时优先于内置算法选择
        /// </summary>
        public ISaveDataEncryption Encryption { get; set; }

        /// <summary>
        /// 当前设置选择的密钥来源，可使用本地会话缓存或项目自定义实现
        /// </summary>
        public ISaveDataKeyProvider EncryptionKeyProvider { get; set; }

        /// <summary>
        /// 当前读写共用的字符串密钥 ID
        /// </summary>
        public string EncryptionKeyId { get; set; } = "";

        /// <summary>
        /// 内置压缩选项，默认不压缩
        /// </summary>
        public SaveDataCompressionMode CompressionMode { get; set; } = SaveDataCompressionMode.None;

        /// <summary>
        /// 可替换写入压缩，提供时优先于内置选项
        /// </summary>
        public ISaveDataCompression Compression { get; set; }

        /// <summary>
        /// 默认本地目录名称，仅允许相对目录且不能穿越持久化根目录
        /// </summary>
        public string DirectoryName { get; set; } = "SaveData";

        /// <summary>
        /// 此会话的项目迁移步骤
        /// </summary>
        public IEnumerable<ISaveDataMigration> Migrations { get; set; } = Array.Empty<ISaveDataMigration>();

        /// <summary>
        /// 单档和解压后的逻辑内容字节上限
        /// </summary>
        public int MaxBytes { get; set; } = 8 * 1024 * 1024;

        /// <summary>
        /// 文档逻辑节点上限
        /// </summary>
        public int MaxNodes { get; set; } = 100000;

        /// <summary>
        /// 正式栏位数量上限
        /// </summary>
        public int MaxSlots { get; set; } = 256;

        /// <summary>
        /// 全部模块共用的就绪期限
        /// </summary>
        public TimeSpan ReadinessTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 可注入实际时间计时服务，空值由平台选择，Web 默认使用 Unity 主循环
        /// </summary>
        public ISaveDataTimeService TimeService { get; set; }

        /// <summary>
        /// 是否要求从初始化线程发起调用
        /// </summary>
        public bool EnforceThreadAffinity { get; set; } = true;

        /// <summary>
        /// 异步入口是否后台调度同步文件 IO 和存档解码，Unity Web 平台关闭
        /// </summary>
        public bool UseBackgroundIO { get; set; } = true;

    }
}
