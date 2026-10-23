using System;
using UnityEngine;

namespace SaveData
{
    /// <summary>
    /// 项目持久化设置资产，支持自定义或本地密钥来源，显式初始化配置可覆盖窗口选项
    /// </summary>
    public sealed class SaveDataSettings : ScriptableObject
    {
        /// <summary>
        /// Resources 中唯一设置资源的名称
        /// </summary>
        public const string ResourceName = "SaveDataSettings";

        /// <summary>
        /// 编辑器设置窗口使用的默认资产路径
        /// </summary>
        public const string AssetPath = "Assets/Resources/SaveDataSettings.asset";

        [Tooltip("编辑器使用的文件格式")]
        [SerializeField]
        private SaveDataFileFormat editorFormat = SaveDataFileFormat.Json;

        [Tooltip("开发构建使用的文件格式")]
        [SerializeField]
        private SaveDataFileFormat developmentFormat = SaveDataFileFormat.Json;

        [Tooltip("发布构建使用的文件格式")]
        [SerializeField]
        private SaveDataFileFormat releaseFormat = SaveDataFileFormat.Bytes;

        [Tooltip("JSON 是否缩进以便调试")]
        [SerializeField]
        private bool prettyPrintJson = true;

        [Tooltip("默认不压缩，可选 MessagePack LZ4 或项目自定义实现")]
        [SerializeField]
        private SaveDataCompressionMode compressionMode = SaveDataCompressionMode.None;

        [Tooltip("自定义压缩类，拖入实现 ISaveDataCompression 且有公开无参构造函数的 C# 脚本")]
        [SerializeField]
        private SaveDataImplementationReference customCompression = new SaveDataImplementationReference();

        [Tooltip("当前读写使用的加密方式，更改加密方式会使旧存档失效")]
        [SerializeField]
        private SaveDataEncryptionMode encryptionMode = SaveDataEncryptionMode.None;

        [Tooltip("自定义加密类，拖入实现 ISaveDataEncryption 的 C# 脚本，支持无参或 ISaveDataKeyProvider 构造函数")]
        [SerializeField]
        private SaveDataImplementationReference customEncryption = new SaveDataImplementationReference();

        [Tooltip("当前读写共用的密钥 ID，读取存档时不会尝试其他密钥")]
        [SerializeField]
        private string encryptionKeyId = "";

        [Tooltip("密钥来源，自定义接受实现 ISaveDataKeyProvider 的普通 C# 脚本，本地使用下方密钥配置")]
        [SerializeField]
        private SaveDataKeyProviderMode keyProviderMode = SaveDataKeyProviderMode.Custom;

        [Tooltip("自定义密钥提供器，拖入实现 ISaveDataKeyProvider 且有公开无参构造函数的 C# 脚本")]
        [SerializeField]
        private SaveDataImplementationReference encryptionKeyProvider = new SaveDataImplementationReference();

        [Tooltip("当前密钥 ID 对应的唯一密钥，使用 Base64 编码，AES 解码后必须为 32 字节，异或必须为非空字节")]
        [SerializeField]
        private string localKeyBase64 = "";

        [Tooltip("本地持久化相对目录")]
        [SerializeField]
        private string directoryName = "SaveData";

        [Tooltip("文件、中间内容和解压后逻辑内容大小限制")]
        [SerializeField]
        private int maxBytes = 8 * 1024 * 1024;

        [Tooltip("文档逻辑节点上限")]
        [SerializeField]
        private int maxNodes = 100000;

        [Tooltip("正式栏位数量上限")]
        [SerializeField]
        private int maxSlots = 16;

        [Tooltip("模块就绪等待秒数，非正值只使用调用方取消")]
        [SerializeField]
        private float readinessTimeoutSeconds = 30;

        [Tooltip("是否约束运行时回调线程")]
        [SerializeField]
        private bool enforceThreadAffinity = true;

        [Tooltip("是否在支持的平台后台执行本地 IO")]
        [SerializeField]
        private bool useBackgroundIO = true;

        [Tooltip("通用项目契约 ID")]
        [SerializeField]
        private string schemaId = "default";

        [Tooltip("通用项目存档结构版本")]
        [SerializeField]
        private int schemaVersion = 1;

        [Tooltip("必须注册的字符串模块 ID")]
        [SerializeField]
        private string[] requiredParticipants = Array.Empty<string>();

        [Tooltip("项目配置提供器，拖入实现 ISaveDataOptionsProvider 且有公开无参构造函数的 C# 脚本，提供器配置会覆盖这里的选项")]
        [SerializeField]
        private SaveDataImplementationReference optionsProvider = new SaveDataImplementationReference();

        /// <summary>
        /// 根据编译环境选取窗口配置的文件格式
        /// </summary>
        public SaveDataFileFormat CurrentFileFormat
        {
            get
            {
#if UNITY_EDITOR
                return editorFormat;
#elif DEVELOPMENT_BUILD
                return developmentFormat;
#else
                return releaseFormat;
#endif
            }
        }

        /// <summary>
        /// 加载项目设置，没有设置资产时使用框架默认选项
        /// </summary>
        public static SaveDataOptions LoadOptions()
        {
            var settings = Resources.Load<SaveDataSettings>(ResourceName);
            if (settings != null)
            {
                return settings.CreateOptions();
            }
            else
            {
                return new SaveDataOptions();
            }
        }

        /// <summary>
        /// 创建独立会话选项并在最后应用项目接口配置
        /// </summary>
        public SaveDataOptions CreateOptions()
        {
            if (float.IsNaN(readinessTimeoutSeconds) || float.IsInfinity(readinessTimeoutSeconds))
            {
                throw new ArgumentException("模块等待超时必须为有限数值");
            }

            var options = new SaveDataOptions
            {
                FileFormat = CurrentFileFormat,
                PrettyPrintJson = prettyPrintJson,
                CompressionMode = compressionMode,
                EncryptionMode = encryptionMode,
                EncryptionKeyId = encryptionKeyId,
                EncryptionKeyProvider = CreateKeyProvider(),
                DirectoryName = directoryName,
                MaxBytes = maxBytes,
                MaxNodes = maxNodes,
                MaxSlots = maxSlots,
                ReadinessTimeout = TimeSpan.FromSeconds(readinessTimeoutSeconds),
                EnforceThreadAffinity = enforceThreadAffinity,
                UseBackgroundIO = useBackgroundIO,
                Schema = new SaveDataSchema(schemaId, schemaVersion, (string[])(requiredParticipants ?? Array.Empty<string>()).Clone())
            };
            if (compressionMode == SaveDataCompressionMode.Custom)
            {
                options.Compression = customCompression?.Create<ISaveDataCompression>();
            }
            if (encryptionMode == SaveDataEncryptionMode.Custom)
            {
                options.Encryption = customEncryption?.Create<ISaveDataEncryption>(options.EncryptionKeyProvider);
            }
            optionsProvider?.Create<ISaveDataOptionsProvider>()?.Configure(options);

            if (options.EncryptionKeyProvider is LocalSaveDataKeyProvider local && !(options.Encryption is NoSaveDataEncryption)
                && (options.Encryption != null || options.EncryptionMode != SaveDataEncryptionMode.None))
            {
                byte[] key = local.GetKey(options.EncryptionKeyId);
                try
                {
                    if ((options.Encryption is Aes256GcmSaveDataEncryption || options.Encryption == null && options.EncryptionMode == SaveDataEncryptionMode.Aes256Gcm)
                        && key.Length != 32)
                        throw new System.Security.Cryptography.CryptographicException("AES-256-GCM 密钥必须为 32 字节");
                }
                finally
                {
                    Array.Clear(key, 0, key.Length);
                }
            }

            return options;
        }

        /// <summary>
        /// 仅创建当前选中的密钥来源
        /// </summary>
        private ISaveDataKeyProvider CreateKeyProvider()
        {
            switch (keyProviderMode)
            {
                case SaveDataKeyProviderMode.Custom:
                    return encryptionKeyProvider?.Create<ISaveDataKeyProvider>();

                case SaveDataKeyProviderMode.Local:
                    if (encryptionMode == SaveDataEncryptionMode.None
                        && string.IsNullOrWhiteSpace(encryptionKeyId)
                        && string.IsNullOrWhiteSpace(localKeyBase64))
                        return null;

                    return new LocalSaveDataKeyProvider(encryptionKeyId, localKeyBase64);

                default:
                    throw new ArgumentOutOfRangeException(nameof(keyProviderMode));
            }
        }

    }
}
