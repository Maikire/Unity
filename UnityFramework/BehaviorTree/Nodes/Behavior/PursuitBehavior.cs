using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 追逐行为
    /// </summary>
    public class PursuitBehavior : NPCBehaviorNode
    {
        public override bool IgnoreWalls => true;

        public override BTState TickNode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            if (Vector3.Distance(data.owner.position, data.foundTargets[0].position) <= data.attackNodeDistance)
            {
                return BTState.Success;
            }

            data.anim.SetBool(data.character.PlayerAnimationParameter.run, true);
            data.driver.MoveToTarget(data.foundTargets[0].position, data.attackNodeDistance, data.moveSpeed);

            return BTState.Running;
        }

        public override void ExitNode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            base.ExitNode(bt);
            data.driver.MoveToTarget(data.owner.position, 1, data.moveSpeed);
        }

    }
}
