using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 本地与服务器共用的异步读取契约，缺档时抛出 FileNotFoundException
    /// </summary>
    public interface ISaveDataReader
    {
        /// <summary>
        /// 检查正式档或备份是否存在
        /// </summary>
        Task<bool> ExistsAsync(string slotId, bool backup, CancellationToken cancellationToken);

        /// <summary>
        /// 读取完整字节，返回值归调用方独占
        /// </summary>
        Task<byte[]> ReadAsync(string slotId, bool backup, CancellationToken cancellationToken);

        /// <summary>
        /// 列出正式档字符串 ID，不包含扩展名
        /// </summary>
        Task<string[]> GetSaveNamesAsync(CancellationToken cancellationToken);

    }
}
