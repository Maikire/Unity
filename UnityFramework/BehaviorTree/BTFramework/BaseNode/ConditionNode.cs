using System;

namespace BehaviorTree
{
    /// <summary>
    /// 条件节点
    /// </summary>
    public abstract class ConditionNode : BTNode
    {
        /// <summary>
        /// 条件
        /// </summary>
        protected abstract Func<BehaviorTree, bool> Condition { get; }

        public override BTState TickNode(BehaviorTree bt)
        {
            if (Condition.Invoke(bt))
            {
                return State = BTState.Success;
            }
            else
            {
                return State = BTState.Failure;
            }
        }

    }
}
