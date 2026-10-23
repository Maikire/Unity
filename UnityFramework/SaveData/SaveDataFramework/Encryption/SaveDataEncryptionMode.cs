namespace SaveData
{
    /// <summary>
    /// 内置加密选择，自定义实现通过会话配置提供
    /// </summary>
    public enum SaveDataEncryptionMode
    {
        /// <summary>
        /// 不加密
        /// </summary>
        None,
        /// <summary>
        /// 循环异或混淆
        /// </summary>
        Xor,
        /// <summary>
        /// AES 256 位密钥和 GCM 认证加密
        /// </summary>
        Aes256Gcm,
        /// <summary>
        /// 使用项目注入的加密实现
        /// </summary>
        Custom
    }
}
