using UnityEngine;

namespace AI.FSM
{
    /// <summary>
    /// 到达目标
    /// </summary>
    public class ReachTargetTrigger : FSMTrigger
    {
        public override FSMTriggerID TriggerID => FSMTriggerID.ReachTarget;

        public override bool HandleTrigger(FSMBase fsm)
        {
            return Vector3.Distance(fsm.transform.position, fsm.FoundTarget.position) <= fsm.Data.AttackStateDistance;
        }


    }
}

