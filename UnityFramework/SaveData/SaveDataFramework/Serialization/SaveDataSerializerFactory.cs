using System;

namespace SaveData
{
    /// <summary>
    /// 根据会话选项组合逻辑编码、压缩、文件封装、整文件加密
    /// </summary>
    public static class SaveDataSerializerFactory
    {
        /// <summary>
        /// 解析自动格式，开发构建和编辑器使用 JSON，发布构建使用 bytes
        /// </summary>
        public static SaveDataFileFormat ResolveFormat(SaveDataFileFormat format)
        {
            if (!Enum.IsDefined(typeof(SaveDataFileFormat), format))
            {
                throw new ArgumentOutOfRangeException(nameof(format));
            }

            if (format != SaveDataFileFormat.Automatic)
            {
                return format;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return SaveDataFileFormat.Json;
#else
            return SaveDataFileFormat.Bytes;
#endif
        }

        /// <summary>
        /// 项目 Serializer 作为内部编码器，自定义压缩和加密优先于内置选项
        /// </summary>
        public static ISaveDataSerializer Create(SaveDataOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!Enum.IsDefined(typeof(SaveDataEncryptionMode), options.EncryptionMode))
            {
                throw new ArgumentOutOfRangeException(nameof(options.EncryptionMode));
            }

            if (!Enum.IsDefined(typeof(SaveDataCompressionMode), options.CompressionMode))
            {
                throw new ArgumentOutOfRangeException(nameof(options.CompressionMode));
            }

            ISaveDataEncryption encryption = options.Encryption;
            if (encryption == null)
            {
                switch (options.EncryptionMode)
                {
                    case SaveDataEncryptionMode.None:
                        encryption = new NoSaveDataEncryption();
                        break;
                    case SaveDataEncryptionMode.Xor:
                        encryption = new XorSaveDataEncryption(options.EncryptionKeyProvider);
                        break;
                    case SaveDataEncryptionMode.Aes256Gcm:
                        encryption = new Aes256GcmSaveDataEncryption(options.EncryptionKeyProvider);
                        break;
                    default:
                        throw new ArgumentException("自定义加密必须提供 Encryption 实现", nameof(options));
                }
            }
            ISaveDataCompression compression = options.Compression;
            if (compression == null)
            {
                switch (options.CompressionMode)
                {
                    case SaveDataCompressionMode.None:
                        compression = new NoSaveDataCompression();
                        break;
                    case SaveDataCompressionMode.Lz4:
                        compression = new MessagePackLz4SaveDataCompression();
                        break;
                    default:
                        throw new ArgumentException("自定义压缩必须提供 Compression 实现", nameof(options));
                }
            }

            return new SaveDataFileSerializer(options.Serializer ?? new JsonSaveDataSerializer(options.PrettyPrintJson),
                            ResolveFormat(options.FileFormat), encryption, options.EncryptionKeyId,
                            options.MaxBytes, compression, options.PrettyPrintJson);
        }

    }
}
