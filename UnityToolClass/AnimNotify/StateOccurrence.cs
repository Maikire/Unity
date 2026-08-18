using System;

namespace Common
{
    /// <summary>
    /// 标识某个持续通知在指定动画循环中的一次活动实例
    /// </summary>
    internal readonly struct StateOccurrence : IEquatable<StateOccurrence>
    {
        /// <summary>
        /// 获取持续通知在配置列表中的索引
        /// </summary>
        public int NotifyIndex { get; }

        /// <summary>
        /// 获取持续通知所属的动画循环索引
        /// </summary>
        public int LoopIndex { get; }

        /// <summary>
        /// 初始化持续通知活动实例标识
        /// </summary>
        public StateOccurrence(int notifyIndex, int loopIndex)
        {
            NotifyIndex = notifyIndex;
            LoopIndex = loopIndex;
        }

        /// <summary>
        /// 判断当前活动实例标识是否与另一个标识相等
        /// </summary>
        public bool Equals(StateOccurrence other)
        {
            return NotifyIndex == other.NotifyIndex && LoopIndex == other.LoopIndex;
        }

        /// <summary>
        /// 判断当前活动实例标识是否与指定对象相等
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is StateOccurrence other && Equals(other);
        }

        /// <summary>
        /// 获取当前活动实例标识的哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(NotifyIndex, LoopIndex);
        }

    }
}
