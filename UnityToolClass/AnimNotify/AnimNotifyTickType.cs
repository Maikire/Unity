namespace Common
{
    /// <summary>
    /// 决定动画通知何时交给接收器
    /// </summary>
    public enum AnimNotifyTickType
    {
        /// <summary>
        /// 动画状态完成本帧求值后在 LateUpdate 中按时间顺序发送
        /// </summary>
        Queued,

        /// <summary>
        /// 跨过通知时间时立即发送，命中窗口和投射物生成等关键玩法通知应使用此类型
        /// </summary>
        BranchingPoint
    }
}
