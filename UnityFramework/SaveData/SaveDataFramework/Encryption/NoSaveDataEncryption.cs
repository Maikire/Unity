using System;

namespace SaveData
{
    /// <summary>
    /// 默认的无加密实现，复制字节以隔离调用方缓冲区
    /// </summary>
    public sealed class NoSaveDataEncryption : ISaveDataEncryption
    {
        /// <summary>
        /// 返回独立明文字节
        /// </summary>
        public byte[] Encrypt(byte[] plaintext, string keyId, byte[] associatedData) =>
            (byte[])(plaintext ?? throw new ArgumentNullException(nameof(plaintext))).Clone();

        /// <summary>
        /// 返回独立明文字节
        /// </summary>
        public byte[] Decrypt(byte[] ciphertext, string keyId, byte[] associatedData) =>
            (byte[])(ciphertext ?? throw new ArgumentNullException(nameof(ciphertext))).Clone();

    }
}
