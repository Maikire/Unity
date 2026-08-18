namespace Common
{
    /// <summary>
    /// 描述播放区间内等待发送的一次通知事件
    /// </summary>
    internal readonly struct ScheduledEvent
    {
        /// <summary>
        /// 获取通知在完整播放时间轴上的触发时间
        /// </summary>
        public float Time { get; }

        /// <summary>
        /// 获取通知生命周期阶段
        /// </summary>
        public AnimNotifyPhase Phase { get; }

        /// <summary>
        /// 获取通知配置
        /// </summary>
        public AnimNotify Notify { get; }

        /// <summary>
        /// 获取通知在配置列表中的索引
        /// </summary>
        public int NotifyIndex { get; }

        /// <summary>
        /// 获取通知所属的动画循环索引
        /// </summary>
        public int LoopIndex { get; }

        /// <summary>
        /// 初始化等待发送的通知事件
        /// </summary>
        public ScheduledEvent(
            float time,
            AnimNotifyPhase phase,
            AnimNotify notify,
            int notifyIndex,
            int loopIndex)
        {
            Time = time;
            Phase = phase;
            Notify = notify;
            NotifyIndex = notifyIndex;
            LoopIndex = loopIndex;
        }

        /// <summary>
        /// 按触发时间和生命周期阶段比较两个通知事件
        /// </summary>
        public static int Compare(ScheduledEvent left, ScheduledEvent right)
        {
            int timeComparison = left.Time.CompareTo(right.Time);
            if (timeComparison != 0)
            {
                return timeComparison;
            }

            int phaseComparison = GetPhaseOrder(left.Phase).CompareTo(GetPhaseOrder(right.Phase));
            return phaseComparison != 0 ? phaseComparison : left.NotifyIndex.CompareTo(right.NotifyIndex);
        }

        /// <summary>
        /// 获取通知生命周期阶段的排序优先级
        /// </summary>
        private static int GetPhaseOrder(AnimNotifyPhase phase)
        {
            return phase switch
            {
                AnimNotifyPhase.StateEnd => 0,
                AnimNotifyPhase.StateBegin => 1,
                AnimNotifyPhase.Notify => 2,
                _ => 3
            };
        }

    }
}
