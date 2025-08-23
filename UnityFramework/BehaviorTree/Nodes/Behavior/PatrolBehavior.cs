using System;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// 巡逻行为
    /// </summary>
    public class PatrolBehavior : BehaviorNode
    {
        public override bool IgnoreWalls => false;

        /// <summary>
        /// 路点数组的索引
        /// </summary>
        private int Index = 0;

        public override void EnterNode(BehaviorTree bt)
        {
            base.EnterNode(bt);
            bt.blackboard.anim.SetBool(bt.blackboard.character.PlayerAnimationParameter.Idle, true);
        }

        public override BTState TickNode(BehaviorTree bt)
        {
            switch (bt.blackboard.patrolMode)
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
            base.ExitNode(bt);
            bt.MoveToTarget(bt.transform.position, 1, bt.blackboard.moveSpeed);
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
            if (Index >= bt.blackboard.wayPoints.Length)
            {
                return BTState.Success;
            }
            if (Vector3.Distance(bt.transform.position, bt.blackboard.wayPoints[Index].position) <= bt.blackboard.patrolEffect)
            {
                Index++;
            }
            bt.MoveToTarget(bt.blackboard.wayPoints[Index].position, 0, bt.blackboard.moveSpeed);

            return BTState.Running;
        }

        /// <summary>
        /// 循环模式
        /// </summary>
        private BTState LoopMode(BehaviorTree bt)
        {
            if (Vector3.Distance(bt.transform.position, bt.blackboard.wayPoints[Index].position) <= bt.blackboard.patrolEffect)
            {
                Index = (Index + 1) % bt.blackboard.wayPoints.Length;
            }
            bt.MoveToTarget(bt.blackboard.wayPoints[Index].position, 0, bt.blackboard.moveSpeed);

            return BTState.Running;
        }

        /// <summary>
        /// 往返模式
        /// </summary>
        private BTState RoundTripMode(BehaviorTree bt)
        {
            if (Vector3.Distance(bt.transform.position, bt.blackboard.wayPoints[Index].position) <= bt.blackboard.patrolEffect)
            {
                if (Index >= bt.blackboard.wayPoints.Length - 1)
                {
                    Array.Reverse(bt.blackboard.wayPoints);
                    Index++;
                }
                Index = (Index + 1) % bt.blackboard.wayPoints.Length;
            }
            bt.MoveToTarget(bt.blackboard.wayPoints[Index].position, 0, bt.blackboard.moveSpeed);

            return BTState.Running;
        }

    }
}
