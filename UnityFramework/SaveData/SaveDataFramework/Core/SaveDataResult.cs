using System;

namespace SaveData
{
    /// <summary>
    /// 存档操作通知和 Try 接口的结果，异常原文用于项目诊断
    /// </summary>
    public sealed class SaveDataResult
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string Operation { get; internal set; }

        /// <summary>
        /// 规范字符串栏位 ID
        /// </summary>
        public string SlotId { get; internal set; }

        /// <summary>
        /// 是否成功完成
        /// </summary>
        public bool Success => Error == null;

        /// <summary>
        /// 项目可根据错误类别选择提示和重试方式
        /// </summary>
        public SaveDataErrorCode ErrorCode => Error switch
        {
            null => SaveDataErrorCode.None,
            System.IO.FileNotFoundException _ => SaveDataErrorCode.NotFound,
            System.IO.DirectoryNotFoundException _ => SaveDataErrorCode.NotFound,
            SaveDataRestoreException _ => SaveDataErrorCode.RestoreFailed,
            System.IO.InvalidDataException _ => SaveDataErrorCode.InvalidData,
            System.Security.Cryptography.CryptographicException _ => SaveDataErrorCode.EncryptionFailed,
            OperationCanceledException _ => SaveDataErrorCode.Cancelled,
            TimeoutException _ => SaveDataErrorCode.TimedOut,
            NotSupportedException _ => SaveDataErrorCode.Unsupported,
            System.IO.IOException _ => SaveDataErrorCode.IOFailure,
            InvalidOperationException _ => SaveDataErrorCode.InvalidOperation,
            ArgumentException _ => SaveDataErrorCode.InvalidOperation,
            _ => SaveDataErrorCode.Unexpected
        };

        /// <summary>
        /// 是否使用了完整默认快照
        /// </summary>
        public bool UsedDefaults { get; internal set; }

        /// <summary>
        /// 是否读取或修复了备份
        /// </summary>
        public bool UsedBackup { get; internal set; }

        /// <summary>
        /// 失败异常，成功时为空
        /// </summary>
        public Exception Error { get; internal set; }

        /// <summary>
        /// 失败后运行时是否仍然一致
        /// </summary>
        public bool RuntimeStateConsistent { get; internal set; } = true;

    }
}
