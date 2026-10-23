using System;
using UnityEngine;

namespace SaveData
{
    /// <summary>
    /// 默认本地桥接
    /// </summary>
    public static class SaveDataUnityBridge
    {
        /// <summary>
        /// 创建默认本地后端，Web 写操作成功返回前等待 IndexedDB 持久化确认
        /// </summary>
        public static ISaveDataStorage CreateLocalStorage(SaveDataOptions options, string extension)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            string directory = SaveDataPathHelper.GetPersistentDirectoryPath(options.DirectoryName);
            var files = new FileSaveDataStorage(directory, extension, options.MaxBytes);

#if UNITY_WEBGL && !UNITY_EDITOR
            return new SynchronizedFileSaveDataStorage(files, new UnityWebSaveDataFileSynchronizer(directory));
#else
            return files;
#endif
        }

        /// <summary>
        /// Web 玩家使用主循环实际时间，其他平台使用系统计时服务
        /// </summary>
        public static ISaveDataTimeService CreateTimeService()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new UnityMainLoopSaveDataTimeService();
#else
            return new SystemSaveDataTimeService();
#endif
        }

        /// <summary>
        /// Web 平台不能通过线程池执行文件 IO
        /// </summary>
        public static bool SupportsBackgroundIO
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return false;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// 新的运行会话重置静态注册，项目应在自己的启动流程重新初始化和注册模块
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlayMode() => SaveDataManager.ResetSession();

    }
}
