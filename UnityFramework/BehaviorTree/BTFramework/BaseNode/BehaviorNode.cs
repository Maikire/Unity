namespace BehaviorTree
{
    /// <summary>
    /// 行为节点
    /// </summary>
    public abstract class BehaviorNode : BTNode
    {
        /// <summary>
        /// true: 搜索目标时忽略墙体
        /// </summary>
        public virtual bool IgnoreWalls { get => false; }

    }
}
