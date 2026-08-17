using UnityEngine;

namespace Common
{
    /// <summary>
    /// AssetBundle 缓存记录
    /// </summary>
    public sealed class BundleRecord
    {
        /// <summary>
        /// AssetBundle 对象
        /// </summary>
        public AssetBundle bundle;

        /// <summary>
        /// 当前有多少个加载请求持有这个 Bundle
        /// </summary>
        public int refCount;

        public BundleRecord() { }

        public BundleRecord(AssetBundle bundle, int refCount)
        {
            this.bundle = bundle;
            this.refCount = refCount;
        }

    }
}
