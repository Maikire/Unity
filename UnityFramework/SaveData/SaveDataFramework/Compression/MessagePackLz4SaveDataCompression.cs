using MessagePack;
using MessagePack.Formatters;
using System;
using System.IO;

namespace SaveData
{
    /// <summary>
    /// MessagePack LZ4 块封装，仅处理 JSON 字节数组，使用静态 formatter 兼容 IL2CPP
    /// </summary>
    public sealed class MessagePackLz4SaveDataCompression : ISaveDataCompression
    {
        /// <summary>
        /// 固定 byte 数组 formatter
        /// </summary>
        private sealed class ByteArrayResolver : IFormatterResolver
        {
            /// <summary>
            /// 返回插件内置 byte 数组 formatter
            /// </summary>
            public IMessagePackFormatter<T> GetFormatter<T>()
            {
                if (typeof(T) == typeof(byte[]))
                {
                    return (IMessagePackFormatter<T>)(object)ByteArrayFormatter.Instance;
                }
                else
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// MessagePack 官方保留的 LZ4 块扩展类型
        /// </summary>
        private const sbyte lz4BlockType = 99;

        /// <summary>
        /// 注册 byte 数组的静态解析器
        /// </summary>
        private static readonly IFormatterResolver resolver = new ByteArrayResolver();

        /// <summary>
        /// 将原始 JSON 字节交给 MessagePack 的 LZ4 块压缩
        /// </summary>
        public byte[] Compress(byte[] input, int maxBytes)
        {
            ValidateInput(input, maxBytes);
            try
            {
                byte[] output = MessagePackSerializer.Serialize<byte[]>(input, CreateOptions(PackedLength(input.Length)));
                if (output.Length > maxBytes)
                {
                    throw new InvalidDataException("MessagePack 压缩结果超过字节上限");
                }

                return output;
            }
            catch (MessagePackSerializationException exception)
            {
                throw new InvalidDataException("MessagePack LZ4 压缩失败", exception);
            }
        }

        /// <summary>
        /// 分配前检查内外层原始长度，只接受单个 LZ4 块及精确长度的 byte 数组
        /// </summary>
        public byte[] Decompress(byte[] input, int uncompressedLength, int maxBytes)
        {
            ValidateInput(input, maxBytes);
            if (uncompressedLength < 0 || uncompressedLength > maxBytes)
            {
                throw new InvalidDataException("MessagePack 解压长度超过字节上限");
            }

            int expectedPackedLength = PackedLength(uncompressedLength);
            byte[] output = null;
            try
            {
                var reader = new MessagePackReader(new ReadOnlyMemory<byte>(input));
                if (reader.NextMessagePackType != MessagePackType.Extension)
                {
                    throw new InvalidDataException("MessagePack 内容不是 LZ4 块");
                }

                ExtensionResult extension = reader.ReadExtensionFormat();
                if (!reader.End || extension.TypeCode != lz4BlockType)
                {
                    throw new InvalidDataException("MessagePack LZ4 类型无效或包含尾随内容");
                }

                var lengthReader = new MessagePackReader(extension.Data);
                if (lengthReader.ReadInt32() != expectedPackedLength)
                {
                    throw new InvalidDataException("MessagePack 内部解压长度与声明不符");
                }

                reader = new MessagePackReader(new ReadOnlyMemory<byte>(input));
                output = MessagePackSerializer.Deserialize<byte[]>(ref reader, CreateOptions(expectedPackedLength));
                if (!reader.End || output == null || output.Length != uncompressedLength)
                {
                    throw new InvalidDataException("MessagePack 解压内容长度无效");
                }

                return output;
            }
            catch (Exception exception)
            {
                if (output != null)
                {
                    Array.Clear(output, 0, output.Length);
                }

                if (exception is MessagePackSerializationException || exception is EndOfStreamException)
                {
                    throw new InvalidDataException("MessagePack LZ4 内容损坏", exception);
                }

                throw;
            }
        }

        /// <summary>
        /// 每次使用独立选项
        /// </summary>
        private static MessagePackSerializerOptions CreateOptions(int maximumPackedLength) =>
            new MessagePackSerializerOptions(resolver)
                .WithCompression(MessagePackCompression.Lz4Block)
                .WithCompressionMinLength(1)
                .WithSecurity(MessagePackSecurity.UntrustedData.WithMaximumDecompressedSize(maximumPackedLength))
                .WithSuggestedContiguousMemorySize(Math.Max(256, Math.Min(65536, maximumPackedLength)));

        /// <summary>
        /// 计算 byte 数组在 MessagePack 中的长度，包括 bin8、bin16 或 bin32 头
        /// </summary>
        private static int PackedLength(int length)
        {
            if (length > int.MaxValue - 5)
            {
                throw new InvalidDataException("MessagePack 输入长度不支持");
            }

            if (length <= byte.MaxValue)
            {
                return length + 2;
            }
            else if (length <= ushort.MaxValue)
            {
                return length + 3;
            }
            else
            {
                return length + 5;
            }
        }

        /// <summary>
        /// 验证原始内容与容量上限
        /// </summary>
        private static void ValidateInput(byte[] input, int maxBytes)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (maxBytes < 1 || input.Length > maxBytes)
            {
                throw new InvalidDataException("MessagePack 输入超过字节上限");
            }
        }

    }
}
