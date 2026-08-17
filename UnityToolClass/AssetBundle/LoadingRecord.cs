using UnityEngine;

namespace Common
{
    /// <summary>
    /// AssetBundle 加载记录
    /// </summary>
    public class LoadingRecord
    {
        /// <summary>
        /// 加载的 bundle
        /// </summary>
        public AssetBundle bundle;

        /// <summary>
        /// 是否操作结束
        /// </summary>
        public bool isDone;

        /// <summary>
        /// 加载的引用计数
        /// </summary>
        public int acquireCount;

    }
}
