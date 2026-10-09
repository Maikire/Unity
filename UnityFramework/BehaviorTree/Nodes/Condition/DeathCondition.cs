using System;

namespace BehaviorTree
{
    /// <summary>
    /// 死亡条件
    /// </summary>
    public class DeathCondition : NPCConditionNode
    {
        protected override Func<BehaviorTree, bool> Condition => (bt) =>
        {
            var data = GetBlackboard(bt);
            return data.character.HP <= 0;
        };

    }
}
