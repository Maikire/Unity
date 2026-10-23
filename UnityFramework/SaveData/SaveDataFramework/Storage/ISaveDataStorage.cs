using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 可替换存储后端，完成才返回，持久性按后端声明，确认失败须明确报告未确定结果
    /// </summary>
    public interface ISaveDataStorage : ISaveDataReader
    {
        /// <summary>
        /// 后端是否支持写入
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// 后端是否支持显式备份读取与修复
        /// </summary>
        bool SupportsBackup { get; }

        /// <summary>
        /// 规范化字符串栏位 ID，所有读写使用相同规则
        /// </summary>
        string NormalizeName(string slotId);

        /// <summary>
        /// 先备份已有正式档，再调用编码回调并提交新档
        /// </summary>
        Task WriteAsync(string slotId, Func<byte[]> createData, CancellationToken cancellationToken);

        /// <summary>
        /// 修复正式档并保留有效备份，不要把坏正式档写入备份
        /// </summary>
        Task RepairAsync(string slotId, byte[] validatedBackup, CancellationToken cancellationToken);

        /// <summary>
        /// 删除正式档，调用方指定是否删除备份
        /// </summary>
        Task DeleteAsync(string slotId, bool includeBackup, CancellationToken cancellationToken);

    }
}
