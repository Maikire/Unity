using System;

namespace SaveData
{
    /// <summary>
    /// 可选同步后端契约，服务器后端无需实现，不要在主线程阻塞异步网络请求
    /// </summary>
    public interface ISynchronousSaveDataStorage : ISaveDataStorage
    {
        /// <summary>
        /// 同步检查文件存在
        /// </summary>
        bool Exists(string slotId, bool backup = false);

        /// <summary>
        /// 同步读取字节
        /// </summary>
        byte[] Read(string slotId, bool backup = false);

        /// <summary>
        /// 同步列出栏位
        /// </summary>
        string[] GetSaveNames();

        /// <summary>
        /// 同步执行备份和新档提交
        /// </summary>
        void Write(string slotId, Func<byte[]> createData);

        /// <summary>
        /// 同步修复正式档并保留备份
        /// </summary>
        void Repair(string slotId, byte[] validatedBackup);

        /// <summary>
        /// 同步删除正式档及可选备份
        /// </summary>
        void Delete(string slotId, bool includeBackup);

    }
}
