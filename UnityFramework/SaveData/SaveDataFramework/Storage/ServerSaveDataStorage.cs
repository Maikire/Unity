using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 将项目服务器读取接口接入 Manager，当前适配器只读，写入可另行实现 ISaveDataStorage
    /// </summary>
    public sealed class ServerSaveDataStorage : ISaveDataStorage
    {
        /// <summary>
        /// 项目提供的服务器读取实现
        /// </summary>
        private readonly IServerSaveDataBackend backend;

        /// <summary>
        /// 此适配器不执行服务器写入
        /// </summary>
        public bool CanWrite => false;

        /// <summary>
        /// 服务器是否提供备份读取
        /// </summary>
        public bool SupportsBackup { get; }

        /// <summary>
        /// 配置服务器读取及备份能力
        /// </summary>
        public ServerSaveDataStorage(IServerSaveDataBackend backend, bool supportsBackup = false)
        {
            this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
            SupportsBackup = supportsBackup;
        }

        /// <summary>
        /// 远端 ID 不受本地文件名规则约束，保留大小写
        /// </summary>
        public string NormalizeName(string slotId)
        {
            if (!string.IsNullOrWhiteSpace(slotId) && slotId == slotId.Trim())
            {
                return slotId;
            }
            else
            {
                throw new ArgumentException("服务器栏位 ID 不能为空或含首尾空白", nameof(slotId));
            }
        }

        /// <summary>
        /// 检查远端存档
        /// </summary>
        public Task<bool> ExistsAsync(string slotId, bool backup, CancellationToken cancellationToken) =>
            backend.ExistsAsync(NormalizeName(slotId), backup, cancellationToken);

        /// <summary>
        /// 读取远端存档
        /// </summary>
        public Task<byte[]> ReadAsync(string slotId, bool backup, CancellationToken cancellationToken) =>
            backend.ReadAsync(NormalizeName(slotId), backup, cancellationToken);

        /// <summary>
        /// 列出远端栏位
        /// </summary>
        public Task<string[]> GetSaveNamesAsync(CancellationToken cancellationToken) => backend.GetSaveNamesAsync(cancellationToken);

        /// <summary>
        /// 显式拒绝服务器写入
        /// </summary>
        public Task WriteAsync(string slotId, Func<byte[]> createData, CancellationToken cancellationToken) => throw new NotSupportedException("服务器适配器只读");

        /// <summary>
        /// 显式拒绝服务器修复
        /// </summary>
        public Task RepairAsync(string slotId, byte[] validatedBackup, CancellationToken cancellationToken) => throw new NotSupportedException("服务器适配器只读");

        /// <summary>
        /// 显式拒绝服务器删除
        /// </summary>
        public Task DeleteAsync(string slotId, bool includeBackup, CancellationToken cancellationToken) => throw new NotSupportedException("服务器适配器只读");

    }
}
