using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 文件提交后等待后端持久化确认，只提供异步契约，失败时报告结果
    /// </summary>
    public sealed class SynchronizedFileSaveDataStorage : ISaveDataStorage, ISaveDataStoragePersistence
    {
        /// <summary>
        /// 负责当前会话文件、备份、格式迁移的本地后端
        /// </summary>
        private readonly FileSaveDataStorage files;

        /// <summary>
        /// 负责把会话文件提交到持久存储的项目或平台实现
        /// </summary>
        private readonly ISaveDataFileSynchronizer synchronizer;

        /// <summary>
        /// 此后端允许保存
        /// </summary>
        public bool CanWrite => files.CanWrite;

        /// <summary>
        /// 此后端沿用文件备份能力
        /// </summary>
        public bool SupportsBackup => files.SupportsBackup;

        /// <summary>
        /// 写操作成功返回时已收到持久化确认
        /// </summary>
        public SaveDataPersistenceScope PersistenceScope => SaveDataPersistenceScope.PersistentStorage;

        /// <summary>
        /// 组合文件提交与持久化确认，同步器必须真实确认持久存储，不可提供空实现
        /// </summary>
        public SynchronizedFileSaveDataStorage(FileSaveDataStorage files, ISaveDataFileSynchronizer synchronizer)
        {
            this.files = files ?? throw new ArgumentNullException(nameof(files));
            this.synchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));
        }

        /// <summary>
        /// 沿用本地文件的字符串栏位规则
        /// </summary>
        public string NormalizeName(string slotId) => files.NormalizeName(slotId);

        /// <summary>
        /// 获取当前正式文件或当前写入格式的绝对路径
        /// </summary>
        public string GetPath(string slotId) => files.GetPath(slotId);

        /// <summary>
        /// 检查已装载到当前会话的正式档或备份
        /// </summary>
        public Task<bool> ExistsAsync(string slotId, bool backup, CancellationToken cancellationToken) =>
            files.ExistsAsync(slotId, backup, cancellationToken);

        /// <summary>
        /// 读取已装载到当前会话的完整原始字节
        /// </summary>
        public Task<byte[]> ReadAsync(string slotId, bool backup, CancellationToken cancellationToken) =>
            files.ReadAsync(slotId, backup, cancellationToken);

        /// <summary>
        /// 列出当前会话中的正式栏位
        /// </summary>
        public Task<string[]> GetSaveNamesAsync(CancellationToken cancellationToken) => files.GetSaveNamesAsync(cancellationToken);

        /// <summary>
        /// 在文件提交前响应取消，提交后等待持久化确认
        /// </summary>
        public async Task WriteAsync(string slotId, Func<byte[]> createData, CancellationToken cancellationToken)
        {
            await files.WriteAsync(slotId, createData, cancellationToken);
            await ConfirmPersistenceAsync();
        }

        /// <summary>
        /// 修复已验证的备份并确认修复结果已经持久化
        /// </summary>
        public async Task RepairAsync(string slotId, byte[] validatedBackup, CancellationToken cancellationToken)
        {
            await files.RepairAsync(slotId, validatedBackup, cancellationToken);
            await ConfirmPersistenceAsync();
        }

        /// <summary>
        /// 删除正式档及可选备份，并确认删除已经持久化
        /// </summary>
        public async Task DeleteAsync(string slotId, bool includeBackup, CancellationToken cancellationToken)
        {
            await files.DeleteAsync(slotId, includeBackup, cancellationToken);
            await ConfirmPersistenceAsync();
        }

        /// <summary>
        /// 持久化失败时传播异常
        /// </summary>
        private async Task ConfirmPersistenceAsync()
        {
            try
            {
                await synchronizer.SynchronizeAsync();
            }
            catch (Exception exception)
            {
                throw new SaveDataPersistenceException(exception);
            }
        }

    }
}
