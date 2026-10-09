using System;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 巡逻行为
    /// </summary>
    public class PatrolBehavior : NPCBehaviorNode
    {
        public override bool IgnoreWalls => false;

        /// <summary>
        /// 路点数组的索引
        /// </summary>
        private int Index = 0;

        public override void EnterNode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            base.EnterNode(bt);
            data.anim.SetBool(data.character.PlayerAnimationParameter.idle, true);
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            switch (data.patrolMode)
            {
                case BTPatrolModes.None:
                    return NoneMode(bt);

                case BTPatrolModes.Once:
                    return OnceMode(bt);

                case BTPatrolModes.Loop:
                    return LoopMode(bt);

                case BTPatrolModes.RoundTrip:
                    return RoundTripMode(bt);

                default:
                    return BTState.Failure;
            }
        }

        public override void ExitNode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            base.ExitNode(bt);
            data.driver.MoveToTarget(data.owner.position, 1, data.moveSpeed);
        }

        /// <summary>
        /// 无巡逻模式
        /// </summary>
        private BTState NoneMode(BehaviorTree bt)
        {
            return BTState.Failure;
        }

        /// <summary>
        /// 单次模式
        /// </summary>
        private BTState OnceMode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            if (Index >= data.wayPoints.Length)
            {
                return BTState.Success;
            }
            if (Vector3.Distance(data.owner.position, data.wayPoints[Index].position) <= data.patrolEffect)
            {
                Index++;
            }
            data.driver.MoveToTarget(data.wayPoints[Index].position, 0, data.moveSpeed);

            return BTState.Running;
        }

        /// <summary>
        /// 循环模式
        /// </summary>
        private BTState LoopMode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            if (Vector3.Distance(data.owner.position, data.wayPoints[Index].position) <= data.patrolEffect)
            {
                Index = (Index + 1) % data.wayPoints.Length;
            }
            data.driver.MoveToTarget(data.wayPoints[Index].position, 0, data.moveSpeed);

            return BTState.Running;
        }

        /// <summary>
        /// 往返模式
        /// </summary>
        private BTState RoundTripMode(BehaviorTree bt)
        {
            var data = GetBlackboard(bt);
            if (Vector3.Distance(data.owner.position, data.wayPoints[Index].position) <= data.patrolEffect)
            {
                if (Index >= data.wayPoints.Length - 1)
                {
                    Array.Reverse(data.wayPoints);
                    Index++;
                }
                Index = (Index + 1) % data.wayPoints.Length;
            }
            data.driver.MoveToTarget(data.wayPoints[Index].position, 0, data.moveSpeed);

            return BTState.Running;
        }

    }
}
