using Org.BouncyCastle.Utilities;
using System;
using System.Security.Cryptography;

namespace SaveData
{
    /// <summary>
    /// Bouncy Castle 字节异或封装，兼容循环密钥协议
    /// </summary>
    public sealed class XorSaveDataEncryption : ISaveDataEncryption
    {
        /// <summary>
        /// 项目提供的密钥来源
        /// </summary>
        private readonly ISaveDataKeyProvider keys;

        /// <summary>
        /// 指定项目密钥来源
        /// </summary>
        public XorSaveDataEncryption(ISaveDataKeyProvider keys) =>
            this.keys = keys ?? throw new ArgumentNullException(nameof(keys));

        /// <summary>
        /// 使用 Bouncy Castle 异或接口转换独立输出数组
        /// </summary>
        public byte[] Encrypt(byte[] plaintext, string keyId, byte[] associatedData) => Transform(plaintext, keyId);

        /// <summary>
        /// 使用相同的 Bouncy Castle 异或接口恢复内容
        /// </summary>
        public byte[] Decrypt(byte[] ciphertext, string keyId, byte[] associatedData) => Transform(ciphertext, keyId);

        /// <summary>
        /// 按密钥长度分段调用插件，使用后清理框架持有的密钥副本
        /// </summary>
        private byte[] Transform(byte[] input, string keyId)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            byte[] source = keys.GetKey(keyId);
            if (source == null || source.Length == 0)
            {
                throw new CryptographicException("异或密钥不可用");
            }

            byte[] key = (byte[])source.Clone();
            try
            {
                var result = new byte[input.Length];
                int offset = 0;
                while (offset < input.Length)
                {
                    int count = Math.Min(key.Length, input.Length - offset);
                    Bytes.Xor(count, input, offset, key, 0, result, offset);
                    offset += count;
                }
                return result;
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
            }
        }

    }
}
