using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;

namespace SaveData
{
    /// <summary>
    /// Bouncy Castle AES-256-GCM 封装
    /// </summary>
    public sealed class Aes256GcmSaveDataEncryption : ISaveDataEncryption
    {
        /// <summary>
        /// 96 位随机 nonce 的字节数
        /// </summary>
        private const int nonceLength = 12;

        /// <summary>
        /// 128 位认证标签的字节数
        /// </summary>
        private const int tagLength = 16;

        /// <summary>
        /// 项目提供的密钥来源
        /// </summary>
        private readonly ISaveDataKeyProvider keys;

        /// <summary>
        /// 指定项目密钥来源，加密计算全部交给 Bouncy Castle
        /// </summary>
        public Aes256GcmSaveDataEncryption(ISaveDataKeyProvider keys) =>
            this.keys = keys ?? throw new ArgumentNullException(nameof(keys));

        /// <summary>
        /// 生成随机 nonce，认证元信息并将插件输出排列为历史密文格式
        /// </summary>
        public byte[] Encrypt(byte[] plaintext, string keyId, byte[] associatedData)
        {
            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

            byte[] key = GetKey(keyId);
            byte[] encrypted = null;
            try
            {
                var nonce = new byte[nonceLength];
                new SecureRandom().NextBytes(nonce);
                GcmBlockCipher cipher = CreateCipher(true, key, nonce, associatedData);
                encrypted = new byte[cipher.GetOutputSize(plaintext.Length)];
                int length = cipher.ProcessBytes(plaintext, 0, plaintext.Length, encrypted, 0);
                length += cipher.DoFinal(encrypted, length);
                if (length != plaintext.Length + tagLength)
                {
                    throw new CryptographicException("AES 插件输出长度无效");
                }

                var result = new byte[checked(nonceLength + length)];
                Buffer.BlockCopy(nonce, 0, result, 0, nonceLength);
                Buffer.BlockCopy(encrypted, plaintext.Length, result, nonceLength, tagLength);
                Buffer.BlockCopy(encrypted, 0, result, nonceLength + tagLength, plaintext.Length);
                return result;
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
                if (encrypted != null)
                {
                    Array.Clear(encrypted, 0, encrypted.Length);
                }
            }
        }

        /// <summary>
        /// 恢复插件密文排列，完成标签认证后才返回明文，失败时清理输出
        /// </summary>
        public byte[] Decrypt(byte[] ciphertext, string keyId, byte[] associatedData)
        {
            if (ciphertext == null || ciphertext.Length < nonceLength + tagLength)
            {
                throw new CryptographicException("AES 密文长度无效");
            }

            byte[] key = GetKey(keyId);
            var plaintext = new byte[ciphertext.Length - nonceLength - tagLength];
            var encrypted = new byte[ciphertext.Length - nonceLength];
            try
            {
                var nonce = new byte[nonceLength];
                Buffer.BlockCopy(ciphertext, 0, nonce, 0, nonceLength);
                Buffer.BlockCopy(ciphertext, nonceLength + tagLength, encrypted, 0, plaintext.Length);
                Buffer.BlockCopy(ciphertext, nonceLength, encrypted, plaintext.Length, tagLength);
                GcmBlockCipher cipher = CreateCipher(false, key, nonce, associatedData);
                int length = cipher.ProcessBytes(encrypted, 0, encrypted.Length, plaintext, 0);
                length += cipher.DoFinal(plaintext, length);
                if (length != plaintext.Length)
                {
                    throw new CryptographicException("AES 插件输出长度无效");
                }

                return plaintext;
            }
            catch (InvalidCipherTextException exception)
            {
                Array.Clear(plaintext, 0, plaintext.Length);
                throw new CryptographicException("AES 存档认证失败", exception);
            }
            catch
            {
                Array.Clear(plaintext, 0, plaintext.Length);
                throw;
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
                Array.Clear(encrypted, 0, encrypted.Length);
            }
        }

        /// <summary>
        /// 为每次操作创建独立插件实例，避免并发操作共享密码状态
        /// </summary>
        private static GcmBlockCipher CreateCipher(bool encrypt, byte[] key, byte[] nonce, byte[] associatedData)
        {
            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(encrypt, new AeadParameters(new KeyParameter(key), tagLength * 8, nonce, associatedData));
            return cipher;
        }

        /// <summary>
        /// 严格要求 32 字节密钥，复制后交给插件
        /// </summary>
        private byte[] GetKey(string keyId)
        {
            byte[] source = keys.GetKey(keyId);
            if (source == null || source.Length != 32)
            {
                throw new CryptographicException("AES-256-GCM 必须使用 32 字节密钥");
            }

            return (byte[])source.Clone();
        }

    }
}
