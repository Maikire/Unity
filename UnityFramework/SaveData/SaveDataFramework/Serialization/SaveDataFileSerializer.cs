using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SaveData
{
    /// <summary>
    /// 文件编码管线，逻辑内容先压缩并封装为完整文件，再对整个文件加密
    /// </summary>
    public sealed class SaveDataFileSerializer : ISaveDataSerializer
    {
        /// <summary>
        /// 标识二进制存档格式
        /// </summary>
        private static readonly byte[] magic =
        {
            // ASCII 字符 SDBF，SaveData Binary Format
            83, 68, 66, 70
        };

        /// <summary>
        /// 支持压缩信息的内部文件协议版本
        /// </summary>
        private const int compressionEnvelopeVersion = 2;

        /// <summary>
        /// JSON 外层包装的保留标记
        /// </summary>
        private const string marker = SaveDataDocument.EnvelopeMarker;

        /// <summary>
        /// 严格 UTF8 编码
        /// </summary>
        private static readonly UTF8Encoding utf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// 内部逻辑编码器，默认使用 Newtonsoft JSON
        /// </summary>
        private readonly ISaveDataSerializer payloadSerializer;

        /// <summary>
        /// 输出文件格式
        /// </summary>
        private readonly SaveDataFileFormat format;

        /// <summary>
        /// 当前会话读写共用的加密实现
        /// </summary>
        private readonly ISaveDataEncryption encryption;

        /// <summary>
        /// 本次写入使用的压缩实现
        /// </summary>
        private readonly ISaveDataCompression compression;

        /// <summary>
        /// 当前会话读写共用的字符串密钥 ID
        /// </summary>
        private readonly string keyId;

        /// <summary>
        /// 文件、中间内容和解压后内容的字节上限
        /// </summary>
        private readonly int maxBytes;

        /// <summary>
        /// JSON 包装是否缩进，默认开启
        /// </summary>
        private readonly bool prettyPrintJson;

        /// <summary>
        /// 所有文件格式、压缩和加密配置统一使用 .dat 正式档扩展名
        /// </summary>
        public string FileExtension => FileSaveDataStorage.PrimaryFileExtension;

        /// <summary>
        /// 配置编码管线，解密只使用当前会话的加密实现和密钥 ID
        /// </summary>
        public SaveDataFileSerializer(ISaveDataSerializer payloadSerializer, SaveDataFileFormat format,
            ISaveDataEncryption encryption = null, string keyId = "", int maxBytes = 8 * 1024 * 1024,
            ISaveDataCompression compression = null,
            bool prettyPrintJson = true)
        {
            this.payloadSerializer = payloadSerializer ?? throw new ArgumentNullException(nameof(payloadSerializer));
            if (format != SaveDataFileFormat.Json && format != SaveDataFileFormat.Bytes)
            {
                throw new ArgumentOutOfRangeException(nameof(format));
            }

            if (maxBytes < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBytes));
            }

            this.format = format;
            this.encryption = encryption ?? new NoSaveDataEncryption();
            this.compression = compression ?? new NoSaveDataCompression();

            if (this.encryption is NoSaveDataEncryption)
            {
                this.keyId = "";
            }
            else
            {
                this.keyId = keyId;
            }

            if (!(this.encryption is NoSaveDataEncryption)
                && (string.IsNullOrWhiteSpace(this.keyId) || utf8.GetByteCount(this.keyId) > 512))
                throw new InvalidDataException("当前加密配置的密钥 ID 无效");

            this.maxBytes = maxBytes;
            this.prettyPrintJson = prettyPrintJson;

        }

        /// <summary>
        /// 先生成包含压缩信息的完整文件，再加密全部文件字节
        /// </summary>
        public byte[] Serialize(SaveDataDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            document.ValidateRootExtensions();
            byte[] file = SerializeFile(document);

            if (encryption is NoSaveDataEncryption)
                return file;

            byte[] ciphertext = null;

            try
            {
                ciphertext = encryption.Encrypt(file, keyId, Array.Empty<byte>());
                CheckSize(ciphertext);
                return ciphertext;
            }
            catch
            {
                Clear(ciphertext);
                throw;
            }
            finally
            {
                Clear(file);
            }
        }

        /// <summary>
        /// 按所选格式生成完整明文文件
        /// </summary>
        private byte[] SerializeFile(SaveDataDocument document)
        {
            byte[] plaintext = payloadSerializer.Serialize(document);

            if (format == SaveDataFileFormat.Json && compression is NoSaveDataCompression)
            {
                CheckSize(plaintext);
                return plaintext;
            }

            byte[] compressed = null;

            try
            {
                CheckSize(plaintext);

                int version;
                if (compression is NoSaveDataCompression)
                {
                    version = 1;
                }
                else
                {
                    version = compressionEnvelopeVersion;
                }

                compressed = compression.Compress(plaintext, maxBytes);
                CheckSize(compressed);
                byte[] header = BuildHeader(version, plaintext.Length);
                byte[] output;

                if (format == SaveDataFileFormat.Json)
                {
                    var root = new JObject
                    {
                        [marker] = version
                    };

                    if (version == compressionEnvelopeVersion)
                    {
                        root["uncompressedLength"] = plaintext.Length;
                    }

                    root["payload"] = Convert.ToBase64String(compressed);
                    Formatting formatting;

                    if (prettyPrintJson)
                    {
                        formatting = Formatting.Indented;
                    }
                    else
                    {
                        formatting = Formatting.None;
                    }

                    output = utf8.GetBytes(root.ToString(formatting));
                }
                else
                {
                    output = WriteBinary(header, compressed);
                }

                CheckSize(output);

                return output;
            }
            finally
            {
                Clear(plaintext);
                Clear(compressed);
            }
        }

        /// <summary>
        /// 按当前加密设置解密完整文件
        /// </summary>
        public JObject Deserialize(byte[] bytes)
        {
            CheckSize(bytes);

            if (encryption is NoSaveDataEncryption)
                return ReadFile(bytes);

            byte[] file = encryption.Decrypt(bytes, keyId, Array.Empty<byte>());
            try
            {
                CheckSize(file);
                return ReadFile(file);
            }
            finally
            {
                Clear(file);
            }
        }

        /// <summary>
        /// 解析完整明文文件
        /// </summary>
        private JObject ReadFile(byte[] bytes)
        {
            CheckSize(bytes);

            if (bytes.Length >= magic.Length && bytes.Take(magic.Length).SequenceEqual(magic))
            {
                return ReadBinary(bytes);
            }

            JObject root;
            try
            {
                root = new JsonSaveDataSerializer().Deserialize(bytes);
            }
            catch (InvalidDataException) when (!(payloadSerializer is JsonSaveDataSerializer))
            {
                ValidateCompressionState(false);
                return payloadSerializer.Deserialize(bytes);
            }

            if (root[marker] == null)
            {
                ValidateCompressionState(false);
                return payloadSerializer.Deserialize(bytes);
            }

            int version = ReadInteger(root[marker]);

            if (version != compressionEnvelopeVersion)
                throw new InvalidDataException("JSON 存档包装版本不支持");

            ValidateCompressionState(true);

            if (root.Count != 3 || root["payload"]?.Type != JTokenType.String)
            {
                throw new InvalidDataException("JSON 存档包装无效");
            }

            int uncompressedLength = ReadInteger(root["uncompressedLength"]);
            byte[] payload;

            try
            {
                payload = Convert.FromBase64String(root["payload"].Value<string>());
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException("存档包装的 Base64 无效", exception);
            }

            return ReadPayload(version, uncompressedLength, payload);
        }

        /// <summary>
        /// 在分配字段和内容缓冲区前校验长度
        /// </summary>
        private JObject ReadBinary(byte[] bytes)
        {
            try
            {
                using var stream = new MemoryStream(bytes, false);
                using var reader = new BinaryReader(stream, utf8);
                reader.ReadBytes(magic.Length);
                int version = reader.ReadInt32();
                ValidateVersion(version);
                ValidateCompressionState(version == compressionEnvelopeVersion);
                int uncompressedLength;

                if (version == compressionEnvelopeVersion)
                {
                    uncompressedLength = reader.ReadInt32();
                }
                else
                {
                    uncompressedLength = 0;
                }

                int length = reader.ReadInt32();

                if (length < 1 || length > maxBytes || length != stream.Length - stream.Position)
                {
                    throw new InvalidDataException("字节存档长度无效或包含尾随内容");
                }

                return ReadPayload(version, uncompressedLength, reader.ReadBytes(length));
            }
            catch (Exception exception) when (exception is EndOfStreamException || exception is DecoderFallbackException)
            {
                throw new InvalidDataException("字节存档头无效或被截断", exception);
            }
        }

        /// <summary>
        /// 读取当前明文文件，压缩信息在整文件解密后才使用
        /// </summary>
        private JObject ReadPayload(int version, int uncompressedLength, byte[] payload)
        {
            if (version == compressionEnvelopeVersion && (uncompressedLength < 1 || uncompressedLength > maxBytes))
            {
                throw new InvalidDataException("解压长度无效或超过字节上限");
            }

            CheckSize(payload);

            byte[] plaintext = null;

            try
            {
                int expectedLength;

                if (version == 1)
                {
                    expectedLength = payload.Length;
                }
                else
                {
                    expectedLength = uncompressedLength;
                }

                plaintext = compression.Decompress(payload, expectedLength, maxBytes);
                CheckSize(plaintext);

                if (plaintext.Length != expectedLength)
                {
                    throw new InvalidDataException("解压内容长度与声明不符");
                }

                return payloadSerializer.Deserialize(plaintext);
            }
            finally
            {
                Clear(payload);
                Clear(plaintext);
            }
        }

        /// <summary>
        /// 为完整明文文件写入二进制格式
        /// </summary>
        private static byte[] WriteBinary(byte[] header, byte[] payload)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, utf8, true);
            writer.Write(header);
            writer.Write(payload.Length);
            writer.Write(payload);
            return stream.ToArray();
        }

        /// <summary>
        /// 明文文件头只记录格式版本及可选的压缩信息，启用加密时全部进入密文
        /// </summary>
        private static byte[] BuildHeader(int version, int uncompressedLength)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, utf8, true);
            writer.Write(magic);
            writer.Write(version);

            if (version == compressionEnvelopeVersion)
            {
                writer.Write(uncompressedLength);
            }

            return stream.ToArray();
        }

        /// <summary>
        /// 读取 32 位整数
        /// </summary>
        private static int ReadInteger(JToken token)
        {
            if (token == null || token.Type != JTokenType.Integer
                || !int.TryParse(token.ToString(Formatting.None), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value))
            {
                throw new InvalidDataException("存档包装整数无效");
            }

            return value;
        }

        /// <summary>
        /// 只接受明确支持的外层协议版本
        /// </summary>
        private static void ValidateVersion(int version)
        {
            if (version != 1 && version != compressionEnvelopeVersion)
            {
                throw new InvalidDataException("存档包装版本不支持");
            }
        }

        /// <summary>
        /// 文件的压缩状态必须与当前设置一致，解压只调用当前实现
        /// </summary>
        private void ValidateCompressionState(bool compressed)
        {
            bool configuredCompression = !(compression is NoSaveDataCompression);

            if (compressed != configuredCompression)
            {
                throw new InvalidDataException("存档压缩状态与当前设置不符");
            }
        }

        /// <summary>
        /// 约束字节、压缩内容、密文、最终文件大小
        /// </summary>
        private void CheckSize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > maxBytes)
            {
                throw new InvalidDataException("存档编码内容为空或超过字节上限");
            }
        }

        /// <summary>
        /// 清理框架持有的中间缓冲区
        /// </summary>
        private static void Clear(byte[] bytes)
        {
            if (bytes != null)
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

    }
}
