using System;

namespace BehaviorTree
{
    /// <summary>
    /// 丢失目标
    /// </summary>
    public class LostTargetCondition : NPCConditionNode
    {
        protected override Func<BehaviorTree, bool> Condition => (bt) =>
        {
            var data = GetBlackboard(bt);
            return data.foundTargets == null || data.foundTargets.Length == 0;
        };

    }
}
