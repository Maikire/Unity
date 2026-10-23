using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaveData
{
    /// <summary>
    /// 不依赖线程池唤醒的先进先出操作门，完成源允许当前线程或调用方同步上下文后继续
    /// </summary>
    internal sealed class SaveDataOperationGate
    {
        /// <summary>
        /// 可取消的排队节点
        /// </summary>
        private sealed class Waiter
        {
            /// <summary>
            /// 执行权通知的完成源
            /// </summary>
            internal readonly TaskCompletionSource<bool> Completion = new TaskCompletionSource<bool>();

            /// <summary>
            /// 在尚未获得执行权时所属的队列节点
            /// </summary>
            internal readonly LinkedListNode<Waiter> Node;

            /// <summary>
            /// 建立请求节点
            /// </summary>
            internal Waiter()
            {
                Node = new LinkedListNode<Waiter>(this);
            }

        }

        /// <summary>
        /// 互斥锁
        /// </summary>
        private readonly object sync = new object();

        /// <summary>
        /// 尚未获得执行权的请求
        /// </summary>
        private readonly LinkedList<Waiter> waiting = new LinkedList<Waiter>();

        /// <summary>
        /// 已获得执行权但尚未通知的请求
        /// </summary>
        private readonly Queue<Waiter> grants = new Queue<Waiter>();

        /// <summary>
        /// 是否已经存在执行权所有者
        /// </summary>
        private bool held;

        /// <summary>
        /// 是否正在展开完成通知，避免同步延续形成递归释放链
        /// </summary>
        private bool notifying;

        /// <summary>
        /// 空闲时立即进入，否则按顺序等待，排队取消不会夺取执行权
        /// </summary>
        internal Task WaitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Waiter waiter;
            lock (sync)
            {
                if (!held)
                {
                    held = true;
                    return Task.CompletedTask;
                }
                waiter = new Waiter();
                waiting.AddLast(waiter.Node);
            }
            return WaitForGrantAsync(waiter, cancellationToken);
        }

        /// <summary>
        /// 等待结束时释放
        /// </summary>
        private async Task WaitForGrantAsync(Waiter waiter, CancellationToken cancellationToken)
        {
            using var registration = cancellationToken.Register(() => Cancel(waiter, cancellationToken));
            await waiter.Completion.Task;
        }

        /// <summary>
        /// 只取消仍在排队的请求，已经转移的执行权由操作本身释放
        /// </summary>
        private void Cancel(Waiter waiter, CancellationToken cancellationToken)
        {
            lock (sync)
            {
                if (waiter.Node.List == null)
                {
                    return;
                }
                waiting.Remove(waiter.Node);
            }
            waiter.Completion.TrySetCanceled(cancellationToken);
        }

        /// <summary>
        /// 将执行权交给下一请求，通过迭代展开通知来避免大量同步请求造成栈溢出
        /// </summary>
        internal void Release()
        {
            lock (sync)
            {
                if (!held)
                {
                    throw new InvalidOperationException("没有可释放的存档执行权");
                }

                if (waiting.Count == 0)
                {
                    held = false;
                    return;
                }

                Waiter next = waiting.First.Value;
                waiting.RemoveFirst();
                grants.Enqueue(next);

                if (notifying)
                {
                    return;
                }

                notifying = true;
            }

            while (true)
            {
                Waiter next;

                lock (sync)
                {
                    if (grants.Count == 0)
                    {
                        notifying = false;
                        return;
                    }
                    next = grants.Dequeue();
                }

                next.Completion.TrySetResult(true);
            }
        }

    }
}
