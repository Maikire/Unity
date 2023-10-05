using UnityEngine;

namespace AI.FSM
{
    /// <summary>
    /// 巡逻
    /// </summary>
    public class PatrolTrigger : FSMTrigger
    {
        /// <summary>
        /// 计时器
        /// </summary>
        private float Timer;

        public override FSMTriggerID TriggerID => FSMTriggerID.Patrol;

        public override void Init()
        {
            Timer = 0;
        }

        public override bool HandleTrigger(FSMBase fsm)
        {
            if (fsm.Data.PatrolMode == FSMPatrolModes.None)
            {
                return false;
            }
            
            Timer += Time.deltaTime;

            if (Timer >= 1)
            {
                if (Random.Range(0, 1f) <= fsm.Data.PatrolProbability)
                {
                    Timer = 0;
                    return true;
                }

                Timer = 0;
            }

            return false;
        }


    }
}

