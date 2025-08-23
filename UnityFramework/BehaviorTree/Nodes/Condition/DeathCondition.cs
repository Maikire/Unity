using System;

namespace BehaviorTree
{
    /// <summary>
    /// 死亡条件
    /// </summary>
    public class DeathCondition : ConditionNode
    {
        protected override Func<BehaviorTree, bool> Condition => (bt) =>
        {
            return bt.blackboard.character.HP <= 0;
        };

    }
}
