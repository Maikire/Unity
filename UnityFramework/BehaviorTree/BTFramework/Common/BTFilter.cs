namespace BehaviorTree
{
    /// <summary>
    /// 过滤结构
    /// </summary>
    public struct BTFilter
    {
        /// <summary>
        /// 条件
        /// </summary>
        public ConditionNode condition;
        /// <summary>
        /// 子节点
        /// </summary>
        public BTNode child;

    }
}
