namespace BehaviorTree
{
    /// <summary>
    /// NPC 条件节点，提供业务黑板
    /// </summary>
    public abstract class NPCConditionNode : ConditionNode
    {
        protected Blackboard GetBlackboard(BehaviorTree bt)
        {
            return bt.GetBlackboard<Blackboard>();
        }


    }
}
