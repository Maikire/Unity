using System;

namespace BehaviorTree
{
    /// <summary>
    /// 丢失目标
    /// </summary>
    public class LostTargetCondition : ConditionNode
    {
        protected override Func<BehaviorTree, bool> Condition => (bt) =>
        {
            return bt.blackboard.foundTargets == null || bt.blackboard.foundTargets.Length == 0;
        };

    }
}
