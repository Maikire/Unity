namespace BehaviorTree
{
    /// <summary>
    /// NPC 行为节点，提供业务黑板及感知策略
    /// </summary>
    public abstract class NPCBehaviorNode : BehaviorNode
    {
        /// <summary>
        /// 搜索目标时是否忽略墙体
        /// </summary>
        public virtual bool IgnoreWalls => false;

        protected Blackboard GetBlackboard(BehaviorTree bt)
        {
            return bt.GetBlackboard<Blackboard>();
        }


    }
}
