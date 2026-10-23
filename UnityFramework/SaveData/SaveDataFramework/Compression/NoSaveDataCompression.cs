using System;
using System.IO;

namespace SaveData
{
    /// <summary>
    /// 无压缩实现，保持输入内容并返回独立数组
    /// </summary>
    public sealed class NoSaveDataCompression : ISaveDataCompression
    {
        /// <summary>
        /// 检查容量后复制未压缩字节
        /// </summary>
        public byte[] Compress(byte[] input, int maxBytes)
        {
            Validate(input, maxBytes);
            return (byte[])input.Clone();
        }

        /// <summary>
        /// 校验声明长度与原始长度一致后复制字节
        /// </summary>
        public byte[] Decompress(byte[] input, int uncompressedLength, int maxBytes)
        {
            Validate(input, maxBytes);
            if (uncompressedLength != input.Length)
            {
                throw new InvalidDataException("未压缩内容长度与声明不符");
            }

            return (byte[])input.Clone();
        }

        /// <summary>
        /// 在分配副本前验证内容和容量
        /// </summary>
        private static void Validate(byte[] input, int maxBytes)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (maxBytes < 1 || input.Length > maxBytes)
            {
                throw new InvalidDataException("压缩内容超过字节上限");
            }
        }

    }
}
