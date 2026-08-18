namespace Common
{
    /// <summary>
    /// 一次动画通知所处的生命周期阶段
    /// </summary>
    public enum AnimNotifyPhase
    {
        /// <summary>
        /// 表示瞬时通知阶段
        /// </summary>
        Notify,

        /// <summary>
        /// 表示持续通知开始阶段
        /// </summary>
        StateBegin,

        /// <summary>
        /// 表示持续通知更新阶段
        /// </summary>
        StateTick,

        /// <summary>
        /// 表示持续通知结束阶段
        /// </summary>
        StateEnd
    }
}
