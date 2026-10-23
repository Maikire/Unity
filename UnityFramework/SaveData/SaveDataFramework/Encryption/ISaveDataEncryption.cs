namespace SaveData
{
    /// <summary>
    /// 独立字节加密契约，实现必须可在后台线程调用且不能修改传入缓冲区
    /// </summary>
    public interface ISaveDataEncryption
    {
        /// <summary>
        /// 按当前配置的密钥 ID 加密完整文件并返回独立密文
        /// </summary>
        byte[] Encrypt(byte[] plaintext, string keyId, byte[] associatedData);

        /// <summary>
        /// 按当前配置的密钥 ID 解密并返回独立明文，认证失败必须抛出异常
        /// </summary>
        byte[] Decrypt(byte[] ciphertext, string keyId, byte[] associatedData);

    }
}
