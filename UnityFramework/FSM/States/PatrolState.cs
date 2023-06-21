using System;
using UnityEngine;

namespace AI.FSM
{
    /// <summary>
    /// 巡逻状态
    /// </summary>
    public class PatrolState : FSMState
    {
        public override FSMStateID StateID => FSMStateID.Patrol;
        public override bool IgnoreWalls => false;

        /// <summary>
        /// 路点数组的索引
        /// </summary>
        private int Index;

        public override void Init()
        {
            Index = 0;
        }

        public override void EnterState(FSMBase fsm)
        {
            base.EnterState(fsm);
            fsm.Data.CompletePatrol = false;
        }

        public override void ActionState(FSMBase fsm)
        {
            base.ActionState(fsm);

            switch (fsm.Data.PatrolMode)
            {
                case FSMPatrolModes.None:
                    NoneMode(fsm);
                    break;
                case FSMPatrolModes.Once:
                    OnceMode(fsm);
                    break;
                case FSMPatrolModes.Loop:
                    LoopMode(fsm);
                    break;
                case FSMPatrolModes.RoundTrip:
                    RoundTripMode(fsm);
                    break;
            }
        }

        public override void ExitState(FSMBase fsm)
        {
            base.ExitState(fsm);
            fsm.MoveToTarget(fsm.transform.position, 1, fsm.Data.MoveSpeed);
        }

        /// <summary>
        /// 无巡逻模式
        /// </summary>
        private void NoneMode(FSMBase fsm)
        {
            fsm.Data.CompletePatrol = true;
        }

        /// <summary>
        /// 单次模式
        /// </summary>
        private void OnceMode(FSMBase fsm)
        {
            if (Vector3.Distance(fsm.transform.position, fsm.Data.WayPoints[Index].position) <= fsm.Data.PatrolEffect)
            {
                Index++;
            }

            if (Index >= fsm.Data.WayPoints.Length)
            {
                fsm.Data.CompletePatrol = true;
                return;
            }

            fsm.MoveToTarget(fsm.Data.WayPoints[Index].position, 0, fsm.Data.MoveSpeed);
        }

        /// <summary>
        /// 循环模式
        /// </summary>
        private void LoopMode(FSMBase fsm)
        {
            if (Vector3.Distance(fsm.transform.position, fsm.Data.WayPoints[Index].position) <= fsm.Data.PatrolEffect)
            {
                Index = (Index + 1) % fsm.Data.WayPoints.Length;
            }

            fsm.MoveToTarget(fsm.Data.WayPoints[Index].position, 0, fsm.Data.MoveSpeed);
        }

        /// <summary>
        /// 往返模式
        /// </summary>
        private void RoundTripMode(FSMBase fsm)
        {
            if (Vector3.Distance(fsm.transform.position, fsm.Data.WayPoints[Index].position) <= fsm.Data.PatrolEffect)
            {
                if (Index >= fsm.Data.WayPoints.Length - 1)
                {
                    Array.Reverse(fsm.Data.WayPoints);
                    Index++;
                }

                Index = (Index + 1) % fsm.Data.WayPoints.Length;
            }

            fsm.MoveToTarget(fsm.Data.WayPoints[Index].position, 0, fsm.Data.MoveSpeed);
        }


    }
}

