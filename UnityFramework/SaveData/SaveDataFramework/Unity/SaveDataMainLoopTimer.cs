using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveData
{
    /// <summary>
    /// 跨场景的主循环计时组件
    /// </summary>
    internal sealed class SaveDataMainLoopTimer : MonoBehaviour
    {
        /// <summary>
        /// 独立的时间期限
        /// </summary>
        private sealed class PendingDelay
        {
            /// <summary>
            /// 包含暂停时间的期限
            /// </summary>
            internal readonly double Deadline;

            /// <summary>
            /// 等待实际时长的调用方任务
            /// </summary>
            internal readonly TaskCompletionSource<bool> Completion = new TaskCompletionSource<bool>();

            /// <summary>
            /// 用于主动结束计时的取消注册
            /// </summary>
            internal readonly CancellationTokenRegistration Registration;

            /// <summary>
            /// 创建期限、显式取消、完成等待
            /// </summary>
            internal PendingDelay(double deadline, CancellationToken cancellationToken)
            {
                Deadline = deadline;
                Registration = cancellationToken.Register(() => Completion.TrySetCanceled(cancellationToken));
            }

        }

        /// <summary>
        /// 当前玩家的唯一计时组件
        /// </summary>
        private static SaveDataMainLoopTimer instance;

        /// <summary>
        /// 由主线程维护的计时请求
        /// </summary>
        private readonly List<PendingDelay> pending = new List<PendingDelay>();

        /// <summary>
        /// 在主线程获取或创建跨场景计时组件
        /// </summary>
        internal static SaveDataMainLoopTimer GetInstance()
        {
            if (SynchronizationContext.Current == null)
            {
                throw new InvalidOperationException("Unity 主循环计时需要从 Unity 主线程调用");
            }

            if (instance == null)
            {
                var host = new GameObject("SaveData Main Loop Timer");
                host.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(host);
                instance = host.AddComponent<SaveDataMainLoopTimer>();
            }

            return instance;
        }

        /// <summary>
        /// 注册实际时间期限，取消直接完成任务并在下一次更新清理注册
        /// </summary>
        internal Task Schedule(TimeSpan duration, CancellationToken cancellationToken)
        {
            var delay = new PendingDelay(Time.realtimeSinceStartupAsDouble + duration.TotalSeconds, cancellationToken);
            pending.Add(delay);
            return delay.Completion.Task;
        }

        /// <summary>
        /// 在完成任务前移除请求，避免延续中的新请求改变当前遍历
        /// </summary>
        private void Update()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            for (int index = pending.Count - 1; index >= 0; index--)
            {
                PendingDelay delay = pending[index];
                if (!delay.Completion.Task.IsCompleted && now < delay.Deadline)
                {
                    continue;
                }

                pending.RemoveAt(index);
                delay.Registration.Dispose();
                delay.Completion.TrySetResult(true);
            }
        }

        /// <summary>
        /// 组件销毁时结束所有等待
        /// </summary>
        private void OnDestroy()
        {
            PendingDelay[] remaining = pending.ToArray();
            pending.Clear();
            if (instance == this)
            {
                instance = null;
            }

            foreach (PendingDelay delay in remaining)
            {
                delay.Registration.Dispose();
                delay.Completion.TrySetCanceled();
            }
        }

    }
}
