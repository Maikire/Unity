using System;
using System.Security.Cryptography;

namespace SaveData
{
    /// <summary>
    /// 初始化时解码唯一的本地密钥，运行时读取独立缓存并支持后台查询
    /// </summary>
    public sealed class LocalSaveDataKeyProvider : ISaveDataKeyProvider
    {
        /// <summary>
        /// 此会话唯一的本地密钥 ID
        /// </summary>
        private readonly string keyId;

        /// <summary>
        /// 初始化时从 Base64 解码的独立密钥副本
        /// </summary>
        private readonly byte[] key;

        /// <summary>
        /// 创建唯一的本地密钥快照，拒绝空 ID、无效 Base64 和空密钥
        /// </summary>
        public LocalSaveDataKeyProvider(string keyId, string keyBase64)
        {
            if (string.IsNullOrWhiteSpace(keyId))
                throw new ArgumentException("本地密钥 ID 不能为空", nameof(keyId));

            this.keyId = keyId;
            try
            {
                key = Convert.FromBase64String(keyBase64 ?? "");
            }
            catch (FormatException exception)
            {
                throw new CryptographicException("本地密钥必须使用有效的 Base64 编码", exception);
            }
            if (key.Length == 0)
                throw new CryptographicException("本地密钥不能为空");
        }

        /// <summary>
        /// 只返回配置中唯一密钥的独立副本，其他 ID 明确报告密钥不可用
        /// </summary>
        public byte[] GetKey(string keyId)
        {
            if (!string.Equals(this.keyId, keyId, StringComparison.Ordinal))
                throw new CryptographicException("本地密钥 ID 与当前配置不匹配");

            return (byte[])key.Clone();
        }

    }
}
