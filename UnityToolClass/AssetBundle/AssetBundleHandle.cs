using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// AssetBundle 加载结果的句柄，包含加载完成后的 AssetBundle 和引用的所有 Bundle Key
    /// </summary>
    public sealed class AssetBundleHandle : IDisposable
    {
        /// <summary>
        /// 加载完成前是否已经请求释放
        /// </summary>
        internal bool releaseRequested;

        /// <summary>
        /// 获取引用的所有 Bundle Key，包括依赖 Bundle 和目标 Bundle
        /// </summary>
        internal List<string> acquiredBundles;

        /// <summary>
        /// 请求路径
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 加载完成后的 AssetBundle
        /// </summary>
        public AssetBundle Bundle { get; internal set; }

        /// <summary>
        /// 是否完成加载
        /// </summary>
        public bool IsDone { get; internal set; }

        /// <summary>
        /// 是否已经释放
        /// </summary>
        public bool IsReleased { get; internal set; }

        /// <summary>
        /// 是否持有一个成功加载的结果
        /// </summary>
        public bool IsSuccess => IsDone && !IsReleased && Bundle != null;

        /// <summary>
        /// 加载完成事件
        /// </summary>
        public event Action<AssetBundleHandle> Completed;

        internal AssetBundleHandle(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// 触发加载完成事件
        /// </summary>
        internal void InvokeCompleted()
        {
            Action<AssetBundleHandle> completed = Completed;
            Completed = null;

            completed?.Invoke(this);
        }

        /// <summary>
        /// 释放这一次 Load 获得的所有引用
        /// </summary>
        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// 释放这一次 Load 获得的所有引用
        /// </summary>
        private void Release()
        {
            AssetBundleManager.Release(this);
        }

    }
}
