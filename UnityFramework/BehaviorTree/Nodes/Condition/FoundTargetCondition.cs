using System;

namespace BehaviorTree
{
    /// <summary>
    /// 发现目标
    /// </summary>
    public class FoundTargetCondition : NPCConditionNode
    {
        protected override Func<BehaviorTree, bool> Condition => (bt) =>
        {
            var data = GetBlackboard(bt);
            return data.foundTargets != null && data.foundTargets.Length > 0;
        };

    }
}
