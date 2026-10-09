namespace BehaviorTree
{
    /// <summary>
    /// 行为树节点返回的状态
    /// </summary>
    public enum BTState
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 失败
        /// </summary>
        Failure,
        /// <summary>
        /// 运行
        /// </summary>
        Running,
        /// <summary>
        /// 中断
        /// </summary>
        Abort,

    }
}
