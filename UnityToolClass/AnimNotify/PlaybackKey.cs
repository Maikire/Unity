using System;

namespace Common
{
    /// <summary>
    /// 标识某个 Behaviour 在指定 Animator 层上的播放状态
    /// </summary>
    internal readonly struct PlaybackKey : IEquatable<PlaybackKey>
    {
        /// <summary>
        /// 获取通知时间轴 Behaviour 的实例标识
        /// </summary>
        public int SourceId { get; }

        /// <summary>
        /// 获取 Animator 层索引
        /// </summary>
        public int LayerIndex { get; }

        /// <summary>
        /// 初始化播放状态键
        /// </summary>
        public PlaybackKey(int sourceId, int layerIndex)
        {
            SourceId = sourceId;
            LayerIndex = layerIndex;
        }

        /// <summary>
        /// 判断当前播放状态键是否与另一个键相等
        /// </summary>
        public bool Equals(PlaybackKey other)
        {
            return SourceId == other.SourceId && LayerIndex == other.LayerIndex;
        }

        /// <summary>
        /// 判断当前播放状态键是否与指定对象相等
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is PlaybackKey other && Equals(other);
        }

        /// <summary>
        /// 获取当前播放状态键的哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(SourceId, LayerIndex);
        }

    }
}
