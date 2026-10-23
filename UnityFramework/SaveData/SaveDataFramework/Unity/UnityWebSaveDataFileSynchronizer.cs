using System;
using System.IO;
using System.Threading.Tasks;
#if UNITY_WEBGL && !UNITY_EDITOR
using AOT;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endif

namespace SaveData
{
    /// <summary>
    /// 通过 Web JavaScript 插件等待 IDBFS 提交 IndexedDB
    /// </summary>
    public sealed class UnityWebSaveDataFileSynchronizer : ISaveDataFileSynchronizer
    {
        /// <summary>
        /// 需要由浏览器持久文件系统承载的目录
        /// </summary>
        private readonly string directory;

        /// <summary>
        /// 指定由 Unity 持久化路径创建的绝对目录
        /// </summary>
        public UnityWebSaveDataFileSynchronizer(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Path.IsPathRooted(directory))
            {
                throw new ArgumentException("浏览器同步目录必须为绝对路径", nameof(directory));
            }
            this.directory = directory;
        }

        /// <summary>
        /// Web 中启动持久化并等待浏览器回调，其他平台应提供自己的同步实现
        /// </summary>
        public Task SynchronizeAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var completion = new TaskCompletionSource<bool>();
            string requestId = Guid.NewGuid().ToString("N");
            requests.Add(requestId, completion);
            try
            {
                SaveData_SynchronizeFiles(directory, requestId, callback);
            }
            catch
            {
                requests.Remove(requestId);
                throw;
            }
            return completion.Task;
#else
            throw new NotSupportedException("浏览器文件同步仅支持 Web 玩家");
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// 浏览器持久化回调的原生签名，参数均为临时 UTF8 字符串地址
        /// </summary>
        private delegate void SynchronizationCallback(IntPtr requestIdPointer, IntPtr errorPointer);

        /// <summary>
        /// 按字符串请求标识跟踪尚未确认的持久化操作
        /// </summary>
        private static readonly Dictionary<string, TaskCompletionSource<bool>> requests = new Dictionary<string, TaskCompletionSource<bool>>();

        /// <summary>
        /// 保持原生回调委托存活，避免浏览器异步回调访问已回收的委托
        /// </summary>
        private static readonly SynchronizationCallback callback = CompleteSynchronization;

        /// <summary>
        /// 导入框架随附的浏览器文件系统同步函数
        /// </summary>
        [DllImport("__Internal")]
        private static extern void SaveData_SynchronizeFiles(string directory, string requestId, SynchronizationCallback callback);

        /// <summary>
        /// 在浏览器主线程复制回调字符串并完成唯一请求
        /// </summary>
        [MonoPInvokeCallback(typeof(SynchronizationCallback))]
        private static void CompleteSynchronization(IntPtr requestIdPointer, IntPtr errorPointer)
        {
            string id = Marshal.PtrToStringUTF8(requestIdPointer);
            if (id == null || !requests.TryGetValue(id, out TaskCompletionSource<bool> completion))
            {
                return;
            }
            requests.Remove(id);
            string message = Marshal.PtrToStringUTF8(errorPointer);
            if (string.IsNullOrEmpty(message))
            {
                completion.TrySetResult(true);
            }
            else
            {
                completion.TrySetException(new IOException("浏览器未确认存档持久化：" + message));
            }
        }

#endif

    }
}
