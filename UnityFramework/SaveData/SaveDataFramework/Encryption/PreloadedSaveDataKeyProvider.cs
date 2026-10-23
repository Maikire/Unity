using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 普通 C# 密钥提供器示例，由项目在初始化存档前预加载密钥
    /// </summary>
    public sealed class PreloadedSaveDataKeyProvider : ISaveDataKeyProvider
    {
        /// <summary>
        /// 保护密钥预加载和后台查询的互斥对象
        /// </summary>
        private readonly object sync = new object();

        /// <summary>
        /// 按密钥 ID 索引的缓存
        /// </summary>
        private readonly Dictionary<string, byte[]> keys = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        /// <summary>
        /// 复制项目预先取得的密钥，更新已有项时清理旧副本
        /// </summary>
        public void PreloadKey(string keyId, byte[] key)
        {
            if (string.IsNullOrWhiteSpace(keyId))
                throw new ArgumentException("密钥 ID 不能为空", nameof(keyId));

            if (key == null || key.Length == 0)
                throw new CryptographicException("预加载密钥不能为空");

            byte[] copy = (byte[])key.Clone();
            lock (sync)
            {
                if (keys.TryGetValue(keyId, out byte[] previous))
                {
                    Array.Clear(previous, 0, previous.Length);
                }
                keys[keyId] = copy;
            }
        }

        /// <summary>
        /// 在调用上下文中异步取得网络密钥，成功且未取消时才替换缓存，失败保留已有密钥
        /// </summary>
        public async Task PreloadKeyAsync(string keyId,
            Func<CancellationToken, Task<byte[]>> loadKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyId))
                throw new ArgumentException("密钥 ID 不能为空", nameof(keyId));

            if (loadKey == null)
                throw new ArgumentNullException(nameof(loadKey));

            cancellationToken.ThrowIfCancellationRequested();
            byte[] key = await loadKey(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            PreloadKey(keyId, key);
        }

        /// <summary>
        /// 返回预加载密钥的副本
        /// </summary>
        public byte[] GetKey(string keyId)
        {
            lock (sync)
            {
                if (string.IsNullOrWhiteSpace(keyId) || !keys.TryGetValue(keyId, out byte[] key))
                    throw new CryptographicException("项目尚未预加载对应密钥 ID 的密钥");

                return (byte[])key.Clone();
            }
        }

    }
}
