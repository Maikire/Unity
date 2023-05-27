namespace AI.FSM
{
    /// <summary>
    /// 完成巡逻
    /// </summary>
    public class CompletePatrolTrigger : FSMTrigger
    {
        public override void Init()
        {
            TriggerID = FSMTriggerID.CompletePatrol;
        }

        public override bool HandleTrigger(FSMBase fsm)
        {
            return fsm.Data.CompletePatrol;
        }


    }
}

