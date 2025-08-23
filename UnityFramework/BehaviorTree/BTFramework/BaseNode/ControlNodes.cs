namespace BehaviorTree
{
    /// <summary>
    /// 控制节点（用于控制子节点的执行）
    /// </summary>
    public abstract class ControlNodes : BTNode
    {
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="child"></param>
        public virtual void AddChild(BTNode child) { }

        /// <summary>
        /// 添加过滤器
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="child"></param>
        public virtual void AddChild(BTFilter filter) { }

    }
}
