using System;

namespace BehaviorTree
{
    /// <summary>
    /// 发现目标
    /// </summary>
    public class FoundTargetCondition : ConditionNode
    {
        protected override Func<BehaviorTree, bool> Condition => (bt) =>
        {
            return bt.blackboard.foundTargets != null && bt.blackboard.foundTargets.Length > 0;
        };

    }
}
