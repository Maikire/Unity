using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 追逐行为
    /// </summary>
    public class PursuitBehavior : BehaviorNode
    {
        public override bool IgnoreWalls => true;

        public override BTState TickNode(BehaviorTree bt)
        {
            if (Vector3.Distance(bt.transform.position, bt.blackboard.foundTargets[0].position) <= bt.blackboard.attackNodeDistance)
            {
                return BTState.Success;
            }

            bt.blackboard.anim.SetBool(bt.blackboard.character.PlayerAnimationParameter.Run, true);
            bt.MoveToTarget(bt.blackboard.foundTargets[0].position, bt.blackboard.attackNodeDistance, bt.blackboard.moveSpeed);

            return BTState.Running;
        }

        public override void ExitNode(BehaviorTree bt)
        {
            base.ExitNode(bt);
            bt.MoveToTarget(bt.transform.position, 1, bt.blackboard.moveSpeed);
        }

    }
}
