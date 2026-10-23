using System;
using System.IO;

namespace SaveData
{
    /// <summary>
    /// 文件已在当前会话改变，但持久化没有确认，刷新后的结果不能当作已提交或已回退
    /// </summary>
    public sealed class SaveDataPersistenceException : IOException
    {
        /// <summary>
        /// 保留持久化失败原因，调用方应诊断并重试
        /// </summary>
        public SaveDataPersistenceException(Exception innerException)
            : base("当前会话文件已变更，但后端未确认持久化，刷新或重开后的结果未确定", innerException)
        {
        }

    }
}
